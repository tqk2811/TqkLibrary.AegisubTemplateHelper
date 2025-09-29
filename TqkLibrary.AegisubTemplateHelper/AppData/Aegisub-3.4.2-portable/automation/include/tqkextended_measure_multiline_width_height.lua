local M = {}

-- split literal giữ cả phần rỗng
local function split_literal(s, sep)
    local res = {}
    local pos = 1
    while true do
        local s1, e1 = string.find(s, sep, pos, true)
        if not s1 then
            table.insert(res, string.sub(s, pos))
            break
        end
        table.insert(res, string.sub(s, pos, s1 - 1))
        pos = e1 + 1
    end
    return res
end

-- tách từ dài thành nhiều phần theo usable_w (UTF-8 safe)
local function split_long_word(style, word, usable_w)
    local parts = {}
    local cur = ""
    for c in word:gmatch("[%z\1-\127\194-\244][\128-\191]*") do
        local test = cur .. c
        local w,_ = aegisub.text_extents(style, test)
        if w > usable_w and cur ~= "" then
            table.insert(parts, cur)
            cur = c
        else
            cur = test
        end
    end
    if cur ~= "" then table.insert(parts, cur) end
    return parts
end

-- xử lý line: nếu dài quá thì tách theo \n hoặc space
local function splitSpaceOrN(style, text, usable_w)
    --aegisub.debug.out(0, "splitSpaceOrN: %s\n", text)
    local lines = { text }
    local changed = true

    while changed do
        changed = false
        for i = 1, #lines do
            local cur = lines[i]
            local w,_ = aegisub.text_extents(style, cur)
            if w > usable_w then
                -- nếu có \n -> tách tại \n đầu tiên
                local s,e = string.find(cur, "\\n", 1, true)
                if s then
                    local left = string.sub(cur, 1, s - 1)
                    local right = string.sub(cur, e + 1)
                    local newlines = {}
                    for j = 1, i - 1 do table.insert(newlines, lines[j]) end
                    table.insert(newlines, left)
                    table.insert(newlines, right)
                    for j = i + 1, #lines do table.insert(newlines, lines[j]) end
                    lines = newlines
                    changed = true
                    break
                else
                    -- không có \n, chia theo số từ
                    local size_w,_ = aegisub.text_extents(style, cur)
                    local numLines = math.ceil(size_w / usable_w)
                    if numLines < 2 then numLines = 2 end

                    local words = {}
                    for word in cur:gmatch("%S+") do table.insert(words, word) end

                    if #words == 0 then
                        -- toàn khoảng trắng, chia đôi
                        local len = #cur
                        local mid = math.floor(len / 2)
                        local left = string.sub(cur, 1, mid)
                        local right = string.sub(cur, mid + 1)
                        local newlines = {}
                        for j = 1, i - 1 do table.insert(newlines, lines[j]) end
                        table.insert(newlines, left)
                        table.insert(newlines, right)
                        for j = i + 1, #lines do table.insert(newlines, lines[j]) end
                        lines = newlines
                        changed = true
                        break
                    end

                    if #words == 1 then
                        -- một từ dài -> tách ký tự
                        local parts = split_long_word(style, words[1], usable_w)
                        local newlines = {}
                        for j = 1, i - 1 do table.insert(newlines, lines[j]) end
                        for _, p in ipairs(parts) do table.insert(newlines, p) end
                        for j = i + 1, #lines do table.insert(newlines, lines[j]) end
                        lines = newlines
                        changed = true
                        break
                    else
                        local wordCountPerLine = math.ceil(#words / numLines)
                        local newparts = {}
                        local idx = 1
                        while idx <= #words do
                            local to = math.min(idx + wordCountPerLine - 1, #words)
                            local chunk = {}
                            for k = idx, to do table.insert(chunk, words[k]) end
                            table.insert(newparts, table.concat(chunk, " "))
                            idx = to + 1
                        end
                        local newlines = {}
                        for j = 1, i - 1 do table.insert(newlines, lines[j]) end
                        for _, p in ipairs(newparts) do table.insert(newlines, p) end
                        for j = i + 1, #lines do table.insert(newlines, lines[j]) end
                        lines = newlines
                        changed = true
                        break
                    end
                end
            end
        end
    end

    return lines
end

-- filter chính
function M.measure_multiline_width_height_filter(meta, styles, subs)
    for li = 1, #subs do
        local line = subs[li]
        if line.class == "dialogue" and not line.comment then
            local style = styles[line.style]
            if style then
                local vid_w = meta.res_x
                local ml = (line.margin_l ~= 0 and line.margin_l) or style.margin_l or 0
                local mr = (line.margin_r ~= 0 and line.margin_r) or style.margin_r or 0
                local usable_w = vid_w - ml - mr

                -- tách theo \N cứng trước
                local hard_blocks = split_literal(line.text, "\\N")
                local lines_final = {}
                for _, blk in ipairs(hard_blocks) do
                    local parts = splitSpaceOrN(style, blk, usable_w)
                    for _, p in ipairs(parts) do table.insert(lines_final, p) end
                end

                -- đo lại với joined
                local max_w, total_h = 0, 0
                for _, l in ipairs(lines_final) do
                    local w,h = aegisub.text_extents(style, l)
                    if w > max_w then max_w = w end
                    total_h = total_h + h
                end

                line.extra = line.extra or {}
                line.extra.measured_lines = lines_final
                line.extra.measured_w = max_w
                line.extra.measured_h = total_h
                subs[li] = line
            end
        end
    end
    return subs
end

return M

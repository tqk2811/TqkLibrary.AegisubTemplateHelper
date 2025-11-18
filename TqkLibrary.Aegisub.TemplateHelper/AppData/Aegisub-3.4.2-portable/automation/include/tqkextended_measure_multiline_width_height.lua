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
local function splitSpaceOrN(text, style, maxwidth)
    --aegisub.debug.out(0, "splitSpaceOrN: %s\n", text)
    local chunks = {}
    local softbreaks = {}

    do
        local pos = 1
        while true do
            local s, e = string.find(text, "\\n", pos, true)
            if not s then break end
            table.insert(softbreaks, s)
            pos = e + 1
        end
    end

    local function has_softbreak_before(i)
        for _, sb in ipairs(softbreaks) do
            if sb < i then
                return sb
            end
        end
        return nil
    end

    local linebuf = ""
    local i = 1
while i <= #text do
        local next_space = string.find(text, " ", i, true)
        local next_chunk_end

        if next_space then
            next_chunk_end = next_space
        else
            next_chunk_end = #text + 1
        end

        local chunk = string.sub(text, i, next_chunk_end - 1)
        local newbuf = (linebuf == "") and chunk or (linebuf .. " " .. chunk)

        local w = aegisub.text_extents(style, newbuf)

        if w > maxwidth then
            local sb = has_softbreak_before(i)
            if sb then
                local left = string.sub(text, 1, sb - 1)
                local right = string.sub(text, sb + 2)
                table.insert(chunks, left)
                return merge(chunks, splitSpaceOrN(right, style, maxwidth))
            end

            -- nếu KHÔNG có soft-break → wrap theo từ (giống code cũ)
            if linebuf == "" then
                -- chunk quá to => buộc tách ký tự
                table.insert(chunks, chunk)
                i = next_chunk_end
            else
                table.insert(chunks, linebuf)
                linebuf = chunk
                i = next_chunk_end
            end
        else
            -- không overflow => nối chunk
            linebuf = newbuf
            i = next_chunk_end
        end
    end

    -- đẩy phần cuối vào
    if linebuf ~= "" then
        table.insert(chunks, linebuf)
    end

    return chunks
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
                    local parts = splitSpaceOrN(blk, style, usable_w)
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

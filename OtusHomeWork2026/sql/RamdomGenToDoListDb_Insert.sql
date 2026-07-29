-- ============================================================
-- Генерация тестовых данных:
--   5 пользователей
--   у каждого 2 списка
--   в каждом списке: 10 активных + 7 выполненных задач
--   + 10 активных задач без списка на пользователя
-- Итого: 5 × (2 × 17 + 10) = 220 задач
-- ============================================================

-- Функция gen_random_uuid() встроена в PostgreSQL 13+.
-- Если у вас версия старше — выполните один раз:
-- CREATE EXTENSION IF NOT EXISTS "pgcrypto";

DO $$
DECLARE
    v_user_id      UUID;
    v_list1_id     UUID;
    v_list2_id     UUID;
    v_base_time    TIMESTAMP;
    v_user_name    TEXT;
    v_tg_id        BIGINT;
BEGIN
    -- ========================================================
    -- Цикл по 5 пользователям
    -- ========================================================
    FOR i IN 1..5 LOOP
        -- Генерируем уникальные идентификаторы
        v_user_id  := gen_random_uuid();
        v_list1_id := gen_random_uuid();
        v_list2_id := gen_random_uuid();

        -- "Разные" имена и Telegram ID для наглядности
        v_user_name := 'user_' || i;
        v_tg_id     := 100000000 + i;

        -- Базовое время регистрации — сдвиг по дням для реалистичности
        v_base_time := '2026-01-01 10:00:00'::TIMESTAMP + (i || ' days')::INTERVAL;

        -- ----------------------------------------------------
        -- 1. Вставляем пользователя
        -- ----------------------------------------------------
        INSERT INTO "ToDoUser"
            ("UserId", "TelegramUserName", "RegisteredAt", "TelegramUserId")
        VALUES
            (v_user_id, v_user_name, v_base_time, v_tg_id);

        -- ----------------------------------------------------
        -- 2. Вставляем 2 списка
        -- ----------------------------------------------------
        INSERT INTO "ToDoList" ("Id", "Name", "UserId", "CreatedAt") VALUES
            (v_list1_id, 'Список 1 пользователя ' || i, v_user_id, v_base_time + INTERVAL '1 hour'),
            (v_list2_id, 'Список 2 пользователя ' || i, v_user_id, v_base_time + INTERVAL '2 hours');

        -- ----------------------------------------------------
        -- 3. 10 АКТИВНЫХ задач в списке №1 (State = 0)
        --    generate_series создаёт 10 строк, для каждой
        --    генерируется свой UUID и свой сдвиг по минутам
        -- ----------------------------------------------------
        INSERT INTO "ToDoItem"
            ("GuidId", "CreateAT", "TaskName", "UserId", "State", "ChangedAt", "ListId")
        SELECT
            gen_random_uuid(),
            v_base_time + INTERVAL '3 hours' + (n || ' minutes')::INTERVAL,   -- CreateAT
            'Активная задача №' || n || ' из списка 1 (user ' || i || ')',    -- TaskName
            v_user_id,
            0,                                                                -- Active
            v_base_time + INTERVAL '3 hours' + (n || ' minutes')::INTERVAL,   -- ChangedAt = CreateAT (не менялась)
            v_list1_id
        FROM generate_series(1, 10) AS n;

        -- ----------------------------------------------------
        -- 4. 7 ВЫПОЛНЕННЫХ задач в списке №1 (State = 1)
        --    ChangedAt сдвигаем на +1 день относительно CreateAT,
        --    чтобы показать, что задача была завершена позже
        -- ----------------------------------------------------
        INSERT INTO "ToDoItem"
            ("GuidId", "CreateAT", "TaskName", "UserId", "State", "ChangedAt", "ListId")
        SELECT
            gen_random_uuid(),
            v_base_time + INTERVAL '1 day'  + (n || ' minutes')::INTERVAL,
            'Выполненная задача №' || n || ' из списка 1 (user ' || i || ')',
            v_user_id,
            1,                                                                -- Completed
            v_base_time + INTERVAL '2 days' + (n || ' minutes')::INTERVAL,    -- завершена на следующий день
            v_list1_id
        FROM generate_series(1, 7) AS n;

        -- ----------------------------------------------------
        -- 5. 10 АКТИВНЫХ задач в списке №2
        -- ----------------------------------------------------
        INSERT INTO "ToDoItem"
            ("GuidId", "CreateAT", "TaskName", "UserId", "State", "ChangedAt", "ListId")
        SELECT
            gen_random_uuid(),
            v_base_time + INTERVAL '3 days' + (n || ' minutes')::INTERVAL,
            'Активная задача №' || n || ' из списка 2 (user ' || i || ')',
            v_user_id,
            0,
            v_base_time + INTERVAL '3 days' + (n || ' minutes')::INTERVAL,
            v_list2_id
        FROM generate_series(1, 10) AS n;

        -- ----------------------------------------------------
        -- 6. 7 ВЫПОЛНЕННЫХ задач в списке №2
        -- ----------------------------------------------------
        INSERT INTO "ToDoItem"
            ("GuidId", "CreateAT", "TaskName", "UserId", "State", "ChangedAt", "ListId")
        SELECT
            gen_random_uuid(),
            v_base_time + INTERVAL '4 days' + (n || ' minutes')::INTERVAL,
            'Выполненная задача №' || n || ' из списка 2 (user ' || i || ')',
            v_user_id,
            1,
            v_base_time + INTERVAL '5 days' + (n || ' minutes')::INTERVAL,
            v_list2_id
        FROM generate_series(1, 7) AS n;

        -- ----------------------------------------------------
        -- 7. 10 АКТИВНЫХ задач БЕЗ списка (ListId = NULL)
        -- ----------------------------------------------------
        INSERT INTO "ToDoItem"
            ("GuidId", "CreateAT", "TaskName", "UserId", "State", "ChangedAt", "ListId")
        SELECT
            gen_random_uuid(),
            v_base_time + INTERVAL '6 days' + (n || ' minutes')::INTERVAL,
            'Задача без списка №' || n || ' (user ' || i || ')',
            v_user_id,
            0,
            v_base_time + INTERVAL '6 days' + (n || ' minutes')::INTERVAL,
            NULL                                                              -- задача не привязана к списку
        FROM generate_series(1, 10) AS n;

    END LOOP;
END $$;
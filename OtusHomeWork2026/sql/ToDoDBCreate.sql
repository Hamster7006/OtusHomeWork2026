-- ============================================================
-- 1. Создание таблицы ToDoUser (пользователи)
-- ============================================================
CREATE TABLE "ToDoUser" (
    "UserId"            UUID            NOT NULL,
    "TelegramUserName"  VARCHAR(255)    NULL,
    "RegisteredAt"      TIMESTAMP       NOT NULL,
    "TelegramUserId"    BIGINT          NOT NULL,
    -- Первичный ключ
    CONSTRAINT "PK_ToDoUser" PRIMARY KEY ("UserId")
);

-- Гарантирует, что один Telegram-аккаунт не будет зарегистрирован дважды
CREATE UNIQUE INDEX "IX_ToDoUser_TelegramUserId"
    ON "ToDoUser" ("TelegramUserId");


-- ============================================================
-- 2. Создание таблицы ToDoList (списки задач)
-- ============================================================
CREATE TABLE "ToDoList" (
    "Id"                UUID            NOT NULL,
    "Name"              VARCHAR(255)    NOT NULL,
    "UserId"            UUID            NOT NULL,
    "CreatedAt"         TIMESTAMP       NOT NULL,
    -- Первичный ключ
    CONSTRAINT "PK_ToDoList" PRIMARY KEY ("Id"),
    -- Внешний ключ: список принадлежит пользователю
    CONSTRAINT "FK_ToDoList_ToDoUser" FOREIGN KEY ("UserId")
        REFERENCES "ToDoUser" ("UserId")
        ON DELETE CASCADE
);

-- Ускоряет поиск списков по пользователю и каскадное удаление
CREATE INDEX "IX_ToDoList_UserId"
    ON "ToDoList" ("UserId");


-- ============================================================
-- 3. Создание таблицы ToDoItem (задачи)
-- ============================================================
CREATE TABLE "ToDoItem" (
    "GuidId"            UUID            NOT NULL,
    "CreateAT"          TIMESTAMP       NOT NULL,
    "TaskName"          VARCHAR(500)    NOT NULL,
    "UserId"            UUID            NOT NULL,
    -- Состояние задачи: Active=0, Completed=1 (enum -> INT)
    "State"             INT             NOT NULL,
    "ChangedAt"         TIMESTAMP       NOT NULL,
    "ListId"            UUID            NULL,
    -- Первичный ключ
    CONSTRAINT "PK_ToDoItem" PRIMARY KEY ("GuidId"),
    -- FK: задача принадлежит пользователю
    CONSTRAINT "FK_ToDoItem_ToDoUser" FOREIGN KEY ("UserId")
        REFERENCES "ToDoUser" ("UserId")
        ON DELETE CASCADE,
    -- FK: задача может принадлежать списку (nullable)
    CONSTRAINT "FK_ToDoItem_ToDoList" FOREIGN KEY ("ListId")
        REFERENCES "ToDoList" ("Id")
        ON DELETE SET NULL
);

-- Индексы по внешним ключам
CREATE INDEX "IX_ToDoItem_UserId"
    ON "ToDoItem" ("UserId");

CREATE INDEX "IX_ToDoItem_ListId"
    ON "ToDoItem" ("ListId");
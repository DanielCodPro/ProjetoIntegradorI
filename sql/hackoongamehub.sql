-- Tabela de Salas
        CREATE TABLE IF NOT EXISTS salas (
            id SERIAL primary key,
            name varchar(255) not null,
            descricao text,
            created_at timestamp
        );
select * from salas;

        -- Tabela de Alunos
        CREATE TABLE IF NOT EXISTS alunos (
            id SERIAL primary KEY,
            username varchar(255) unique not null,
            name varchar(255) not null,
            turma varchar(50),
            points INTEGER DEFAULT 0,
            level INTEGER DEFAULT 1,
            xp INTEGER DEFAULT 0,
            stars INTEGER DEFAULT 0,
            id_sala INTEGER references salas(id) on delete set null,
            password varchar(255),
            created_at timestamp
        );
select * from usuarios;
        
        -- Tabela de Professores
        CREATE TABLE IF NOT EXISTS professores (
            id SERIAL primary KEY,
            name varchar(255) not null,
            email varchar(255) unique not null,
            password varchar(255),
            created_at timestamp
        );

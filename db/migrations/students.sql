CREATE TABLE student (
    id uuid NOT NULL,
    name varchar(255) NOT NULL,
    email varchar(255) NOT NULL,
    address varchar(255) NOT NULL,

    CONSTRAINT student_pk PRIMARY KEY(id)
);

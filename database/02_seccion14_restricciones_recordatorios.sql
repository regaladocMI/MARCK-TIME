USE MarcTimeDB;
GO

IF OBJECT_ID('RestriccionesHorarioApp', 'U') IS NULL
BEGIN
    CREATE TABLE RestriccionesHorarioApp (
        RestriccionHorarioAppId INT IDENTITY(1,1) PRIMARY KEY,
        HorarioClaseId INT NOT NULL,
        AplicacionId   INT NOT NULL,
        CONSTRAINT FK_RestriccionesHorarioApp_HorariosClase FOREIGN KEY (HorarioClaseId)
            REFERENCES HorariosClase(HorarioClaseId) ON DELETE CASCADE,
        CONSTRAINT FK_RestriccionesHorarioApp_Aplicaciones FOREIGN KEY (AplicacionId)
            REFERENCES Aplicaciones(AplicacionId) ON DELETE NO ACTION,
        CONSTRAINT UQ_RestriccionesHorarioApp UNIQUE (HorarioClaseId, AplicacionId)
    );
    CREATE INDEX IX_RestriccionesHorarioApp_AplicacionId ON RestriccionesHorarioApp(AplicacionId);
END
GO

IF OBJECT_ID('RecordatoriosTarea', 'U') IS NULL
BEGIN
    CREATE TABLE RecordatoriosTarea (
        RecordatorioTareaId INT IDENTITY(1,1) PRIMARY KEY,
        TareaId             INT NOT NULL,
        MinutosAntelacion   INT NOT NULL,
        Enviado             BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_RecordatoriosTarea_Tareas FOREIGN KEY (TareaId)
            REFERENCES Tareas(TareaId) ON DELETE CASCADE,
        CONSTRAINT UQ_RecordatoriosTarea UNIQUE (TareaId, MinutosAntelacion)
    );
    CREATE INDEX IX_RecordatoriosTarea_Pendientes ON RecordatoriosTarea(TareaId, Enviado);
END
GO
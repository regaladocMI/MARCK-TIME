IF DB_ID('MarcTimeDB') IS NULL
BEGIN
    CREATE DATABASE MarcTimeDB;
END
GO

USE MarcTimeDB;
GO

DROP TABLE IF EXISTS Notificaciones;
DROP TABLE IF EXISTS LogAuditoria;
DROP TABLE IF EXISTS ConfiguracionNotificaciones;
DROP TABLE IF EXISTS Sonidos;
DROP TABLE IF EXISTS TiposEvento;
DROP TABLE IF EXISTS MetasSemanales;
DROP TABLE IF EXISTS ResumenUsoDiario;
DROP TABLE IF EXISTS SesionesUso;
DROP TABLE IF EXISTS Aplicaciones;
DROP TABLE IF EXISTS CategoriasAplicacion;
DROP TABLE IF EXISTS TareasEtiquetas;
DROP TABLE IF EXISTS Etiquetas;
DROP TABLE IF EXISTS Tareas;
DROP TABLE IF EXISTS HorariosClase;
DROP TABLE IF EXISTS Cursos;
DROP TABLE IF EXISTS Periodos;
DROP TABLE IF EXISTS ConfiguracionApp;
DROP TABLE IF EXISTS Usuarios;
GO

/* USUARIOS */
CREATE TABLE Usuarios (
    UsuarioId              INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario          NVARCHAR(50)   NOT NULL,
    CorreoElectronico      NVARCHAR(150)  NOT NULL,
    HashContrasena         NVARCHAR(256)  NOT NULL,
    FechaCreacion          DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    Activo                 BIT            NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Usuarios_Correo UNIQUE (CorreoElectronico),
    CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario)
);
GO

/* CONFIGURACION_APP */
CREATE TABLE ConfiguracionApp (
    ConfiguracionAppId          INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId                   INT            NOT NULL,
    Tema                        NVARCHAR(20)   NOT NULL DEFAULT 'Claro',
    MinutosAntelacionDefecto    INT            NOT NULL DEFAULT 10,
    FechaActualizacion          DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ConfiguracionApp_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT UQ_ConfiguracionApp_Usuario UNIQUE (UsuarioId)
);
GO

/* PERIODOS */
CREATE TABLE Periodos (
    PeriodoId    INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId    INT            NOT NULL,
    Nombre       NVARCHAR(50)   NOT NULL,
    FechaInicio  DATE           NOT NULL,
    FechaFin     DATE           NOT NULL,
    Activo       BIT            NOT NULL DEFAULT 1,
    CONSTRAINT FK_Periodos_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT CK_Periodos_Fechas CHECK (FechaFin > FechaInicio)
);
GO

/* CURSOS */
CREATE TABLE Cursos (
    CursoId    INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId  INT            NOT NULL,
    PeriodoId  INT            NULL,
    Nombre     NVARCHAR(100)  NOT NULL,
    Codigo     NVARCHAR(20)   NULL,
    Color      NVARCHAR(7)    NULL,
    CONSTRAINT FK_Cursos_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_Cursos_Periodos FOREIGN KEY (PeriodoId)
        REFERENCES Periodos(PeriodoId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_Cursos_PeriodoId ON Cursos(PeriodoId);
GO

/* HORARIOS_CLASE */
CREATE TABLE HorariosClase (
    HorarioClaseId   INT IDENTITY(1,1) PRIMARY KEY,
    CursoId          INT            NOT NULL,
    DiaSemana        TINYINT        NOT NULL,
    HoraInicio       TIME           NOT NULL,
    HoraFin          TIME           NOT NULL,
    Ubicacion        NVARCHAR(100)  NULL,
    CONSTRAINT FK_HorariosClase_Cursos FOREIGN KEY (CursoId)
        REFERENCES Cursos(CursoId) ON DELETE CASCADE,
    CONSTRAINT CK_HorariosClase_Dia CHECK (DiaSemana BETWEEN 1 AND 7),
    CONSTRAINT CK_HorariosClase_Horas CHECK (HoraFin > HoraInicio)
);
GO
CREATE INDEX IX_HorariosClase_CursoId_Dia ON HorariosClase(CursoId, DiaSemana);
GO

/* ETIQUETAS */
CREATE TABLE Etiquetas (
    EtiquetaId     INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId      INT            NOT NULL,
    Nombre         NVARCHAR(40)   NOT NULL,
    Color          NVARCHAR(7)    NULL,
    CONSTRAINT FK_Etiquetas_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT UQ_Etiquetas_Usuario_Nombre UNIQUE (UsuarioId, Nombre)
);
GO

/* TAREAS */
CREATE TABLE Tareas (
    TareaId              INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId            INT            NOT NULL,
    CursoId              INT            NULL,
    Titulo               NVARCHAR(150)  NOT NULL,
    Descripcion          NVARCHAR(MAX)  NULL,
    FechaEntrega         DATETIME2      NOT NULL,
    Prioridad            TINYINT        NOT NULL DEFAULT 2,
    Completada           BIT            NOT NULL DEFAULT 0,
    FechaCreacion        DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    FechaActualizacion   DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    RowVersion           ROWVERSION,
    CONSTRAINT FK_Tareas_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_Tareas_Cursos FOREIGN KEY (CursoId)
        REFERENCES Cursos(CursoId) ON DELETE NO ACTION,
    CONSTRAINT CK_Tareas_Prioridad CHECK (Prioridad BETWEEN 1 AND 3)
);
GO
CREATE INDEX IX_Tareas_Usuario_FechaEntrega ON Tareas(UsuarioId, FechaEntrega);
GO

/* TAREAS_ETIQUETAS */
CREATE TABLE TareasEtiquetas (
    TareaId      INT NOT NULL,
    EtiquetaId   INT NOT NULL,
    CONSTRAINT PK_TareasEtiquetas PRIMARY KEY (TareaId, EtiquetaId),
    CONSTRAINT FK_TareasEtiquetas_Tareas FOREIGN KEY (TareaId)
        REFERENCES Tareas(TareaId) ON DELETE CASCADE,
    CONSTRAINT FK_TareasEtiquetas_Etiquetas FOREIGN KEY (EtiquetaId)
        REFERENCES Etiquetas(EtiquetaId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_TareasEtiquetas_EtiquetaId ON TareasEtiquetas(EtiquetaId);
GO

/* CATEGORIAS_APLICACION */
CREATE TABLE CategoriasAplicacion (
    CategoriaAplicacionId  INT IDENTITY(1,1) PRIMARY KEY,
    Nombre                 NVARCHAR(50)  NOT NULL,
    EsProductiva           BIT           NOT NULL DEFAULT 0,
    CONSTRAINT UQ_CategoriasAplicacion_Nombre UNIQUE (Nombre)
);
GO

/* APLICACIONES */
CREATE TABLE Aplicaciones (
    AplicacionId             INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId                INT            NOT NULL,
    CategoriaAplicacionId    INT            NULL,
    NombreEjecutable         NVARCHAR(150)  NOT NULL,
    NombreVisible            NVARCHAR(100)  NOT NULL,
    LimiteMinutosDiarios     INT            NULL,
    Activo                   BIT            NOT NULL DEFAULT 1,
    FechaCreacion            DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    FechaActualizacion       DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    RowVersion               ROWVERSION,
    CONSTRAINT FK_Aplicaciones_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_Aplicaciones_CategoriasAplicacion FOREIGN KEY (CategoriaAplicacionId)
        REFERENCES CategoriasAplicacion(CategoriaAplicacionId) ON DELETE SET NULL,
    CONSTRAINT UQ_Aplicaciones_Usuario_Ejecutable UNIQUE (UsuarioId, NombreEjecutable)
);
GO

/* SESIONES_USO */
CREATE TABLE SesionesUso (
    SesionUsoId        BIGINT IDENTITY(1,1) PRIMARY KEY,
    AplicacionId       INT            NOT NULL,
    FechaHoraInicio    DATETIME2      NOT NULL,
    FechaHoraFin       DATETIME2      NULL,
    DuracionSegundos   AS (DATEDIFF(SECOND, FechaHoraInicio, FechaHoraFin)) PERSISTED,
    TituloVentana      NVARCHAR(300)  NULL,
    CONSTRAINT FK_SesionesUso_Aplicaciones FOREIGN KEY (AplicacionId)
        REFERENCES Aplicaciones(AplicacionId) ON DELETE CASCADE,
    CONSTRAINT CK_SesionesUso_Fechas CHECK (FechaHoraFin IS NULL OR FechaHoraFin >= FechaHoraInicio)
);
GO
CREATE INDEX IX_SesionesUso_Aplicacion_Inicio ON SesionesUso(AplicacionId, FechaHoraInicio);
GO
CREATE UNIQUE INDEX UQ_SesionesUso_UnaActivaPorApp
    ON SesionesUso(AplicacionId)
    WHERE FechaHoraFin IS NULL;
GO

/* RESUMEN_USO_DIARIO */
CREATE TABLE ResumenUsoDiario (
    ResumenUsoDiarioId  BIGINT IDENTITY(1,1) PRIMARY KEY,
    AplicacionId        INT       NOT NULL,
    Fecha               DATE      NOT NULL,
    MinutosTotales      INT       NOT NULL DEFAULT 0,
    CONSTRAINT FK_ResumenUsoDiario_Aplicaciones FOREIGN KEY (AplicacionId)
        REFERENCES Aplicaciones(AplicacionId) ON DELETE CASCADE,
    CONSTRAINT UQ_ResumenUsoDiario_App_Fecha UNIQUE (AplicacionId, Fecha)
);
GO

/* METAS_SEMANALES */
CREATE TABLE MetasSemanales (
    MetaSemanalId            INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId                INT       NOT NULL,
    CategoriaAplicacionId    INT       NULL,
    AplicacionId             INT       NULL,
    MinutosObjetivo          INT       NOT NULL,
    FechaInicioSemana        DATE      NOT NULL,
    CONSTRAINT FK_MetasSemanales_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_MetasSemanales_CategoriasAplicacion FOREIGN KEY (CategoriaAplicacionId)
        REFERENCES CategoriasAplicacion(CategoriaAplicacionId) ON DELETE NO ACTION,
    CONSTRAINT FK_MetasSemanales_Aplicaciones FOREIGN KEY (AplicacionId)
        REFERENCES Aplicaciones(AplicacionId) ON DELETE NO ACTION,
    CONSTRAINT CK_MetasSemanales_UnSoloObjetivo CHECK (
        (CategoriaAplicacionId IS NOT NULL AND AplicacionId IS NULL) OR
        (CategoriaAplicacionId IS NULL AND AplicacionId IS NOT NULL)
    )
);
GO

/* SONIDOS */
CREATE TABLE Sonidos (
    SonidoId          INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId         INT            NULL,
    NombreArchivo     NVARCHAR(150)  NOT NULL,
    RutaArchivo       NVARCHAR(300)  NOT NULL,
    EsPredeterminado  BIT            NOT NULL DEFAULT 0,
    CONSTRAINT FK_Sonidos_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE
);
GO

/* TIPOS_EVENTO */
CREATE TABLE TiposEvento (
    TipoEventoId  INT IDENTITY(1,1) PRIMARY KEY,
    Codigo        NVARCHAR(40)   NOT NULL,
    Descripcion   NVARCHAR(150)  NOT NULL,
    CONSTRAINT UQ_TiposEvento_Codigo UNIQUE (Codigo)
);
GO

/* CONFIGURACION_NOTIFICACIONES */
CREATE TABLE ConfiguracionNotificaciones (
    ConfiguracionNotificacionId  INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId            INT       NOT NULL,
    TipoEventoId         INT       NOT NULL,
    SonidoId             INT       NULL,
    MinutosAntelacion    INT       NOT NULL DEFAULT 10,
    Activo               BIT       NOT NULL DEFAULT 1,
    CONSTRAINT FK_ConfigNotif_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_ConfigNotif_TiposEvento FOREIGN KEY (TipoEventoId)
        REFERENCES TiposEvento(TipoEventoId) ON DELETE CASCADE,
    CONSTRAINT FK_ConfigNotif_Sonidos FOREIGN KEY (SonidoId)
        REFERENCES Sonidos(SonidoId) ON DELETE NO ACTION,
    CONSTRAINT UQ_ConfigNotif_Usuario_Tipo UNIQUE (UsuarioId, TipoEventoId)
);
GO

/* NOTIFICACIONES */
CREATE TABLE Notificaciones (
    NotificacionId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId            INT            NOT NULL,
    TipoEventoId         INT            NOT NULL,
    Mensaje              NVARCHAR(300)  NOT NULL,
    FechaHoraEnvio       DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    Leida                BIT            NOT NULL DEFAULT 0,
    TareaId              INT            NULL,
    HorarioClaseId       INT            NULL,
    AplicacionId         INT            NULL,
    CONSTRAINT FK_Notificaciones_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_Notificaciones_TiposEvento FOREIGN KEY (TipoEventoId)
        REFERENCES TiposEvento(TipoEventoId) ON DELETE NO ACTION,
    CONSTRAINT FK_Notificaciones_Tareas FOREIGN KEY (TareaId)
        REFERENCES Tareas(TareaId) ON DELETE NO ACTION,
    CONSTRAINT FK_Notificaciones_HorariosClase FOREIGN KEY (HorarioClaseId)
        REFERENCES HorariosClase(HorarioClaseId) ON DELETE NO ACTION,
    CONSTRAINT FK_Notificaciones_Aplicaciones FOREIGN KEY (AplicacionId)
        REFERENCES Aplicaciones(AplicacionId) ON DELETE NO ACTION,
    CONSTRAINT CK_Notificaciones_UnaEntidad CHECK (
        (CASE WHEN TareaId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN HorarioClaseId IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN AplicacionId IS NOT NULL THEN 1 ELSE 0 END) <= 1
    )
);
GO
CREATE INDEX IX_Notificaciones_Usuario_Fecha ON Notificaciones(UsuarioId, FechaHoraEnvio);
GO

/* LOG_AUDITORIA */
CREATE TABLE LogAuditoria (
    LogAuditoriaId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId         INT            NOT NULL,
    TablaAfectada     NVARCHAR(60)   NOT NULL,
    AccionRealizada   NVARCHAR(20)   NOT NULL,
    ValorAnterior     NVARCHAR(MAX)  NULL,
    ValorNuevo        NVARCHAR(MAX)  NULL,
    FechaHora         DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_LogAuditoria_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_LogAuditoria_Usuario_Fecha ON LogAuditoria(UsuarioId, FechaHora);
GO

CREATE TRIGGER trg_SesionesUso_ActualizarResumen
ON SesionesUso
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    MERGE ResumenUsoDiario AS destino
    USING (
        SELECT
            AplicacionId,
            CAST(FechaHoraInicio AS DATE) AS Fecha,
            SUM(DATEDIFF(SECOND, FechaHoraInicio, FechaHoraFin)) / 60 AS MinutosNuevos
        FROM inserted
        WHERE FechaHoraFin IS NOT NULL
        GROUP BY AplicacionId, CAST(FechaHoraInicio AS DATE)
    ) AS origen
    ON destino.AplicacionId = origen.AplicacionId AND destino.Fecha = origen.Fecha
    WHEN MATCHED THEN
        UPDATE SET MinutosTotales = destino.MinutosTotales + origen.MinutosNuevos
    WHEN NOT MATCHED THEN
        INSERT (AplicacionId, Fecha, MinutosTotales)
        VALUES (origen.AplicacionId, origen.Fecha, origen.MinutosNuevos);
END
GO
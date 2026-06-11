-- Esquema Access (ACE/JET). Ejecutar en Access para crear tablas.

CREATE TABLE Ubicacion (
    Id AUTOINCREMENT PRIMARY KEY,
    Nombre TEXT(150) NOT NULL,
    Tipo TEXT(20) NOT NULL,
    ParentId LONG,
    Descripcion TEXT(255),
    Latitud DOUBLE,
    Longitud DOUBLE,
    CONSTRAINT FK_Ubicacion_Parent FOREIGN KEY (ParentId) REFERENCES Ubicacion(Id)
);

CREATE TABLE Sitio (
    Id AUTOINCREMENT PRIMARY KEY,
    Nombre TEXT(150) NOT NULL,
    Descripcion LONGTEXT,
    UbicacionId LONG NOT NULL,
    Estado TEXT(30) NOT NULL,
    FechaCreacion DATETIME NOT NULL,
    CONSTRAINT FK_Sitio_Ubicacion FOREIGN KEY (UbicacionId) REFERENCES Ubicacion(Id)
);

CREATE TABLE Costo (
    Id AUTOINCREMENT PRIMARY KEY,
    SitioId LONG NOT NULL,
    Tipo TEXT(50) NOT NULL,
    Valor CURRENCY NOT NULL,
    Moneda TEXT(10) NOT NULL,
    Observacion TEXT(255),
    CONSTRAINT FK_Costo_Sitio FOREIGN KEY (SitioId) REFERENCES Sitio(Id)
);

CREATE TABLE Comida (
    Id AUTOINCREMENT PRIMARY KEY,
    Nombre TEXT(150) NOT NULL
);

CREATE TABLE SitioComida (
    SitioId LONG NOT NULL,
    ComidaId LONG NOT NULL,
    ValorReferencial CURRENCY,
    Observacion TEXT(255),
    CONSTRAINT PK_SitioComida PRIMARY KEY (SitioId, ComidaId),
    CONSTRAINT FK_SitioComida_Sitio FOREIGN KEY (SitioId) REFERENCES Sitio(Id),
    CONSTRAINT FK_SitioComida_Comida FOREIGN KEY (ComidaId) REFERENCES Comida(Id)
);

CREATE TABLE Usuarios (
    Id AUTOINCREMENT PRIMARY KEY,
    Nombre TEXT(150) NOT NULL,
    Email TEXT(150) NOT NULL,
    PasswordHash TEXT(255) NOT NULL,
    Activo YESNO NOT NULL
);

CREATE TABLE Rol (
    Id AUTOINCREMENT PRIMARY KEY,
    Nombre TEXT(100) NOT NULL
);

CREATE TABLE UsuarioRol (
    UsuarioId LONG NOT NULL,
    RolId LONG NOT NULL,
    CONSTRAINT PK_UsuarioRol PRIMARY KEY (UsuarioId, RolId),
    CONSTRAINT FK_UsuarioRol_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_UsuarioRol_Rol FOREIGN KEY (RolId) REFERENCES Rol(Id)
);

CREATE TABLE Permiso (
    Id AUTOINCREMENT PRIMARY KEY,
    Codigo TEXT(100) NOT NULL,
    Descripcion TEXT(255)
);

CREATE TABLE RolPermiso (
    RolId LONG NOT NULL,
    PermisoId LONG NOT NULL,
    CONSTRAINT PK_RolPermiso PRIMARY KEY (RolId, PermisoId),
    CONSTRAINT FK_RolPermiso_Rol FOREIGN KEY (RolId) REFERENCES Rol(Id),
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (PermisoId) REFERENCES Permiso(Id)
);

CREATE TABLE Multimedia (
    Id AUTOINCREMENT PRIMARY KEY,
    SitioId LONG NOT NULL,
    Url TEXT(255) NOT NULL,
    Tipo TEXT(50) NOT NULL,
    Orden LONG NOT NULL,
    CONSTRAINT FK_Multimedia_Sitio FOREIGN KEY (SitioId) REFERENCES Sitio(Id)
);

CREATE TABLE Auditoria (
    Id AUTOINCREMENT PRIMARY KEY,
    UsuarioId LONG NOT NULL,
    Entidad TEXT(100) NOT NULL,
    EntidadId LONG NOT NULL,
    Accion TEXT(50) NOT NULL,
    Fecha DATETIME NOT NULL,
    Datos LONGTEXT NOT NULL,
    CONSTRAINT FK_Auditoria_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

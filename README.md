# MARC TIME

Aplicacion de escritorio (WPF, .NET 10) para gestion de productividad estudiantil:
mide el tiempo de uso por aplicacion, permite configurar limites de tiempo con cierre
automatico, y notifica horarios de clase y tareas pendientes con alertas visuales y sonoras.

## Arquitectura

- **MarcTime.Core** - Modelos de dominio y logica de negocio pura.
- **MarcTime.Data** - Acceso a datos (SQL Server), patron Repository.
- **MarcTime.UI** - WPF, patron MVVM.

## Requisitos

- Visual Studio Community (2022 o superior)
- .NET 10 SDK
- SQL Server (local, via SSMS)

#AUTOR
Reiner Alexander Regalado Cabrera

# Deploy con Docker (VPS)

## Requisitos en el VPS
- Docker Engine y Docker Compose (`docker compose version`)
- Puerto de la API abierto en el firewall (por defecto `5000`) si querés acceder desde fuera

## 1) Subir el proyecto al VPS (desde tu máquina)
Elegí una opción:

**Opción A: rsync (recomendado, no sube bin/obj/.git si respetás .dockerignore/local)**
```bash
rsync -avz --exclude 'bin' --exclude 'obj' --exclude '.env' \
  /home/majo/Documentos/TiendaRopa/ usuario@IP_DEL_VPS:~/TiendaRopa/
```

**Opción B: git clone (si el proyecto está en un repo remoto)**
```bash
ssh usuario@IP_DEL_VPS
git clone <URL_DEL_REPO> ~/TiendaRopa
```

## 2) Configurar variables de entorno (en el VPS)
```bash
cd ~/TiendaRopa
cp .env.example .env
nano .env   # o vi .env: cambiá POSTGRES_PASSWORD por una contraseña segura
```

> No subas `.env` al repositorio. La conexión a la base la arma `docker-compose.yml` usando estas variables.

## 3) Construir y levantar (en el VPS)
```bash
cd ~/TiendaRopa
docker compose up -d --build
```

Ver estado y logs:
```bash
docker compose ps
docker compose logs -f api
docker compose logs -f db
```

La API aplica las migraciones/seed automáticamente al arrancar (con reintentos si la DB tarda en estar lista).

## 4) Probar
Desde el VPS:
```bash
curl http://localhost:5000/
curl http://localhost:5000/prendas
```

Desde tu máquina (si el puerto 5000 está abierto en el VPS):
```bash
curl http://IP_DEL_VPS:5000/prendas
```

> En `Production` el documento OpenAPI no se expone (solo en desarrollo). Los endpoints principales son `/`, `/categorias`, `/prendas`.

## 5) Operaciones comunes
```bash
# Reiniciar
docker compose restart

# Parar (conserva datos en el volumen pgdata)
docker compose down

# Parar y BORRAR datos (cuidado)
docker compose down -v

# Actualizar tras cambios en el código
docker compose up -d --build
```

## Notas
- **DB separada del backend**: Postgres corre en el servicio `db` y los datos persisten en el volumen `pgdata`. El backend se conecta usando el host `db` dentro de la red de Docker.
- El puerto `5432` está publicado en `docker-compose.yml` para facilitar pruebas; si no necesitás acceso externo a Postgres, podés eliminar la sección `ports` del servicio `db`.
- Para producción real, lo habitual es poner un reverse proxy (Nginx/Caddy) con HTTPS delante y no exponer la API directamente. Si querés, lo agregamos después.
- La cadena de conexión puede sobreescribirse con la variable `ConnectionStrings__DefaultConnection` (ya configurada en `docker-compose.yml`).

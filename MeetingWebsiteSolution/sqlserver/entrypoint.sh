set -e
if [ "$1" = '/opt/mssql/bin/sqlservr' ]; then
  if [ ! -f /tmp/app-initialized ]; then
      function initialize_app_database() {
        sleep 10s        
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Password123$ -d master -i initdb.sql -C
        touch /tmp/app-initialized
      }
    initialize_app_database &
  fi
fi
exec "$@"
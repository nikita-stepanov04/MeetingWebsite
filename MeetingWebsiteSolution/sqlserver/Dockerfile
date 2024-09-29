FROM mcr.microsoft.com/mssql/server

ENV ACCEPT_EULA=Y

COPY ./sqlserver /

ENTRYPOINT [ "/bin/bash", "entrypoint.sh" ]
CMD [ "/opt/mssql/bin/sqlservr" ]
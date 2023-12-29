FROM python:3.9-slim-buster as final

WORKDIR /scripts

COPY config/config.py config/config.py
COPY handles/ handles/
COPY spread_amx/config/ spread_amx/config/
COPY spread_amx/services/ spread_amx/services/
COPY spread_amx/main.py spread_amx/main.py
COPY spread_amx/requirements.txt spread_amx/requirements.txt

RUN pip install -r spread_amx/requirements.txt

ARG CSB_GID=2000
ARG BIGMOM_UID=5000

RUN groupadd -g $CSB_GID csb && \
    useradd -U -G csb -d /home/bigmom -m -s /bin/nologin -u $BIGMOM_UID bigmom && \
    chown -R $BIGMOM_UID:$CSB_GID /home/bigmom .
USER $BIGMOM_UID:$CSB_GID

ENV PYTHONPATH=/
CMD python /scripts/spread_amx/main.py
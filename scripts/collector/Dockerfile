FROM python:3.9-alpine3.13 as final

WORKDIR /scripts

COPY config/ config/
COPY collector/ collector/

RUN pip install -r collector/requirements.txt

ARG CSB_GID=2000
ARG BIGMOM_UID=5000

RUN addgroup -g $CSB_GID csb; \
    addgroup -g $BIGMOM_UID bigmom; \
    adduser -G bigmom -h /home/bigmom -s /bin/nologin -u $BIGMOM_UID -D bigmom; \
    addgroup bigmom csb; \
    chown -R $BIGMOM_UID:$CSB_GID /home/bigmom .
USER $BIGMOM_UID:$CSB_GID

ENV PYTHONPATH=/
CMD python /scripts/collector/main.py
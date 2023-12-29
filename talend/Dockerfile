FROM adoptopenjdk/openjdk8:debian-slim
WORKDIR /talend

COPY [ "jobInfo.properties", "jobInfo.properties" ]
COPY [ "lib/", "lib/" ]
COPY [ "Enchainement_v2/", "Enchainement_v2/" ]

ARG CSB_GID=2000
ARG BIGMOM_UID=5000

RUN groupadd -g $CSB_GID csb && \
    useradd -U -G csb -d /home/bigmom -m -s /bin/nologin -u $BIGMOM_UID bigmom && \
    chown -R $BIGMOM_UID:$CSB_GID /home/bigmom . && \
    chmod 775 Enchainement_v2/Enchainement_v2_run.sh
USER $BIGMOM_UID:$CSB_GID

ENTRYPOINT [ "Enchainement_v2/Enchainement_v2_run.sh" ]
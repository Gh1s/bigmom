FROM ghcr.io/oracle/oraclelinux8-instantclient:19

WORKDIR /scripts

COPY config/config.py config/config.py
COPY handles/ handles/
COPY spread_pwc/config/ spread_pwc/config/
COPY spread_pwc/services/ spread_pwc/services/
COPY spread_pwc/main.py spread_pwc/main.py
COPY spread_pwc/requirements.txt spread_pwc/requirements.txt

RUN dnf install -y \
                python39 \
                python39-libs \
                python39-pip \
                python39-setuptools && \
    dnf clean all
RUN /bin/pip3.9 install -r spread_pwc/requirements.txt

ARG CSB_GID=2000
ARG BIGMOM_UID=5000

RUN groupadd -g $CSB_GID csb && \
    useradd -U -G csb -d /home/bigmom -m -s /bin/nologin -u $BIGMOM_UID bigmom && \
    chown -R $BIGMOM_UID:$CSB_GID /home/bigmom .
USER $BIGMOM_UID:$CSB_GID

ENV PYTHONPATH=/
ENV LD_LIBRARY_PATH=/usr/lib/oracle/19.11/client64/lib:$LD_LIBRARY_PATH
CMD /bin/python3.9 /scripts/spread_pwc/main.py
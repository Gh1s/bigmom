# Bigmom

Ce projet contient Bigmom, le middle office monétique.

## 📁 Structure du projet

### `/.config`

Les fichiers de configuration des outils .NET.

### `/data`

Les fichier de données.

### `/docker`

Les fichiers de configuration pour les services Docker.

| Dossier                   | Description                                                                                                          |
| ------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| `/docker/api`             | Les fichiers de configuration pour l'API sur tous les environnements.                                                |
| `/docker/app`             | Les fichiers de configuration pour l'UI sur tous les environnements.                                                 |
| `/docker/certs`           | Les certificats utilisés pour l'environnement `Local`.                                                               |
| `/docker/job-collector`   | Les fichiers de configuration pour le job collector sur tous les environnements.                                     |
| `/docker/job-extract`     | Les fichiers de configuration pour le job d'extraction sur tous les environnements.                                  |
| `/docker/job-integration` | Les fichiers de configuration pour le job d'intégration sur tous les environnements.                                 |
| `/docker/job-spread-amx`  | Les fichiers de configuration pour le job de diffusion des données vers l'intranet AMEX sur tous les environnements. |
| `/docker/job-spread-jcb`  | Les fichiers de configuration pour le job de diffusion des données vers l'intranet JCB sur tous les environnements.  |
| `/docker/job-spread-pwc`  | Les fichiers de configuration pour le job de diffusion des données vers Powercard sur tous les environnements.       |
| `/docker/nginx`           | Les fichiers de configuration du reverse proxy NGINX sur tous les environnements.                                    |
| `/docker/postgres`        | Les fichiers de configuration du serveur de base de données Postgres sur tous les environnements .                   |

Le fichier `docker-compose-build.yaml` permet générer les images Docker de Bigmom.<br />
Le fichier `docker-compose-ci.yaml` permet de lancer les services requis pour l'exécution des tests dans le CI Gitlab.<br />
Le fichier `docker-compose-dev.yaml` permet de lancer les services requis des développements et de l'écriture des tests.<br />
Le fichier `docker-compose-local-app.yaml` permet de lancer l'API, l'UI et le reverse proxy Nginx sur l'environnement `Local`.<br />
Le fichier `docker-compose-local-collector.yaml` permet le job collector sur l'environnement `Local`.<br />
Le fichier `docker-compose-local-extract.yaml` permet de lancer d'extraction sur l'environnement `Local`.<br />
Le fichier `docker-compose-local-integration.yaml` permet de lancer d'intégration sur l'environnement `Local`.<br />
Le fichier `docker-compose-local-spread-jobs.yaml` permet de lancer les jobs de diffusion des données sur l'environnement `Local`.<br />
Le fichier `docker-compose-staging-app.yaml` permet de lancer l'API, l'UI et le reverse proxy Nginx sur l'environnement `Staging`.<br />
Le fichier `docker-compose-staging-collector.yaml` permet le job collector sur l'environnement `Staging`.<br />
Le fichier `docker-compose-staging-extract.yaml` permet de lancer d'extraction sur l'environnement `Staging`.<br />
Le fichier `docker-compose-staging-integration.yaml` permet de lancer d'intégration sur l'environnement `Staging`.<br />
Le fichier `docker-compose-staging-spread-jobs.yaml` permet de lancer les jobs de diffusion des données sur l'environnement `Staging`.<br />
Le fichier `docker-compose-production-app.yaml` permet de lancer l'API, l'UI et le reverse proxy Nginx sur l'environnement `Production`.<br />
Le fichier `docker-compose-production-collector.yaml` permet le job collector sur l'environnement `Production`.<br />
Le fichier `docker-compose-production-extract.yaml` permet de lancer d'extraction sur l'environnement `Production`.<br />
Le fichier `docker-compose-production-integration.yaml` permet de lancer d'intégration sur l'environnement `Production`.<br />
Le fichier `docker-compose-production-spread-jobs.yaml` permet de lancer les jobs de diffusion des données sur l'environnement `Production`.<br />
Le fichier `build.sh` permet de générer les images Docker et de les déployer.

## 🌏 Environnements

Il existe 4 environnements :
- Development : C'est l'environnement par défaut lors des développement.
- Local : C'est l'environnement faire fonctionner Bigmom dans les conditions de la production, mais sur un poste développeur.
- Staging : C'est l'environnement de recette ou de qualification.
- Production : C'est l'environnement de production.

## 🧰 Tooling

### .NET

Le backend du projet est ecris en C# 9 .NET 5.

Pour le compiler il vous faudra le SDK [.NET 5](https://dotnet.microsoft.com/download).

### Node

Le frontend du projet est ecris en [Angular](https://angular.io).

Pour le compiler il vous faudra [Node JS](https://nodejs.org/).

### IDE

Comme IDE vous pouvez utiliser [Visual Studio](https://visualstudio.microsoft.com/) ou [Rider](https://www.jetbrains.com/rider).

## TODO CRAP

```bash
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.integration.commerce.requests.process --create --partitions 6 --replication-factor 1 --config max.message.bytes=5242880
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.integration.tlc.requests.process --create --partitions 6 --replication-factor 1
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.data.requests.process --create --partitions 3 --replication-factor 1
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.data.responses.process --create --partitions 6 --replication-factor 1
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.balancing.requests.process --create --partitions 1 --replication-factor 1 --config retention.ms=86400000
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.spreading.requests.process --create --partitions 3 --replication-factor 1
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.spreading.responses.process --create --partitions 3 --replication-factor 1
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.job.healthchecks.process --create --partitions 1 --replication-factor 1 --config retention.ms=3600000
kafka-topics --zookeeper zookeeper:2181 --topic bigmom.api.healthchecks.process --create --partitions 1 --replication-factor 1 --config retention.ms=3600000
```

```bash
dotnet ef migrations add $MigrationName -s src\Csb.BigMom.Job\ -p src\Csb.BigMom.Infrastructure\
dotnet ef migrations remove -s src\Csb.BigMom.Job\ -p src\Csb.BigMom.Infrastructure\
dotnet ef database update -s src\Csb.BigMom.Job\
```

```bash
groupadd -g 2000 csb
useradd -U -G csb -d /home/bigmom -m -u 5000 bigmom
```

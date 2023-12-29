# Bigmom - Spécifications fonctionnelles

Cette page décrit les spécification fonctionnelles de Bigmom.

1. [Accueil](README.md)
2. [Spécification fonctionnelles](functional_spec.md)
3. [Spécification techniques](technical_spec.md)
4. [Architecture](architecture.md)
5. [Infrastructure](infrastructure.md)

## Plan de télécollectes

Le plan de télécollectes est une chaîne de traitement, dans laquelle Bigmom intervient pour équilibrer les heures de télécollectes des TPEs.

Chaque commerçant, en fonction de son activité, se voit attribuer un [MCC (Merchant Category Code)](https://en.wikipedia.org/wiki/Merchant_category_code).
Ces MCC sont regroupés dans des plages horaires, de telle façon que tous les TPEs des commerçants dont le MCC est contenu dans une plage particulière, verra ses heures de télécollectes attribuées dans cette plage. Il en existe trois :
- 19h - 3h
- 23h - 3h
- 5h30 - 7h

Chaque TPE pa plusieurs applications installées. Lorsque qu'un TPE lance une télécollecte, il le fait pour une application donnée. Deux applications d'un même TPE ne peuvent pas télécollecter en même temps. Lorsque c'est le cas, les télécollectes du TPE pour ces applications entrent en échec. C'est pourquoi Bigmom attribut des heures de télécollectes qui ne se chevauchent pas pour les applications d'un même TPE.
L'écart entre les télécollectes des applications d'un TPE est spécifié en minutes, et est configurable.

Actuellement, Bigmom est configuré pour traiter les applications suivantes :
- EMV
- NFC
- VAD
- JADE
- AMX
- JCB

Pour chaque application, une priorité pour attribuer l'heure de télécollecte est configurée. De cette façon, une application avec une priorité 1 aura toujours son heure de télécollecte attribuée avant une application de priorité 2 ou 3. Attention, cela ne veut pas dire que l'application télécollectera avant, mais uniquement que son heure de télécollecte sera attribuée avant.

Les heures de télécollecte sont définies à la minute. Pour éviter de saturer les serveurs PAD, un nombre maximum de télécollectes par minute est respecté. Ce nombre est configurable dans Bigmom.

Il est également possible de configurer une durée en minutes, pour déterminer la plage maximale que Bigmom est autorisé à utiliser, pour attribuer les heures de télécollectes d'un commerçant. Le début de plage est déterminée à partir de l'heure de télécollecte de la première application du premier TPE du commerçant, et la fin de plage en ajoutant la durée configurée en minute à l'heure de début de plage. Si la plage du commerçant dépasse la plage MCC, celle du commerçant est modifiée pour être contenue dans la plage MCC. Bigmom va ensuite boucler dans la plage du commerçant pour attribuer les heures de télécollectes, ignorant si besoin le nombre de télécollectes maximales par minute.

Il existe une particularité pour les commerçant raccordés en RTC. Ceux là ne peuvent pas avoir deux télécollectes en même temps, peu importe l'application et le TPE sinon elle échoue. Dans ce cas, Bigmom fait en sorte qu'aucune heure de télécollecte pour toutes les applications des tous les TPEs du commerçant ne se chevauchent.

Avec tous ces paramètres, Bigmom calcule un plan de télécollectes pour le produit cartésien des applications de tous les TPEs de tous les commerçant, en excluant celles rattachées à un contrat résilié ou un commerçant résilié.

Pour assurer l'idempotence du calcul, Bigmom trie les données de la façon suivante :
- Par heure de début de plage MCC croissante
- Par identifiant commerçant croissant
- Par n° de site TPE croissant
- Par n° de série TPE croissant
- Par priorité d'application croissante
- Par n° de contrat

## Supervision des télécollectes

TODO: Write functional spec
# AutoSalonPet

Учебный проект: сайт автосалона на Blazor Server, который показывает работу с Kafka
в реальном приложении. Симулятор продаёт машины, события идут через Kafka,
а лента продаж и финансы на сайте строятся из этих событий.

## Что происходит

- Каждые 30 секунд симулятор "продаёт" две машины: новую и с пробегом
- Каждая продажа отправляется в Kafka, топик `sales` (ключ = марка машины)
- Две независимые группы читают один и тот же поток событий:
  - `autosalon-feed` - наполняет ленту продаж (вкладки "Новые" и "С пробегом")
  - `autosalon-finance` - считает выручку и баланс компании

Вкладка "Финансы" обновляется по мере поступления событий.

## Как запустить

Нужен Docker и .NET 10 SDK.

1. Поднять Kafka:

```
docker run -d --name kafka -p 9092:9092 apache/kafka:latest
```

2. Запустить приложение:

```
dotnet run
```

3. Открыть http://localhost:5267

<details>
<summary>Если нужен веб-интерфейс Kafka (Kafka UI)</summary>

```
docker network create kafka-net
docker rm -f kafka
docker run -d --name kafka --network kafka-net -p 9092:9092 -e "KAFKA_NODE_ID=1" -e "KAFKA_PROCESS_ROLES=controller,broker" -e "KAFKA_LISTENERS=PLAINTEXT://:9092,INTERNAL://:29092,CONTROLLER://:9093" -e "KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092,INTERNAL://kafka:29092" -e "KAFKA_LISTENER_SECURITY_PROTOCOL_MAP=CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,INTERNAL:PLAINTEXT" -e "KAFKA_CONTROLLER_LISTENER_NAMES=CONTROLLER" -e "KAFKA_INTER_BROKER_LISTENER_NAME=INTERNAL" -e "KAFKA_CONTROLLER_QUORUM_VOTERS=1@localhost:9093" -e "KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1" apache/kafka:latest
docker run -d --name kafka-ui --network kafka-net -p 8080:8080 -e "KAFKA_CLUSTERS_0_NAME=local" -e "KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS=kafka:29092" provectuslabs/kafka-ui:latest
```

Kafka UI: http://localhost:8080

</details>

## Структура

```
AutoSalonPet/
  Components/    UI на Blazor
  Models/        Car, SaleRecord
  Services/      бизнес-логика и работа с Kafka
  .githooks/     pre-commit хук с автоформатированием
```

## Как устроено

```
SalesSimulatorService (продюсер)
        |
        v
Kafka: топик sales (3 партиции, ключ = марка)
        |
        +--> autosalon-feed    -> SalesFeedService  -> лента продаж
        +--> autosalon-finance -> FinanceService    -> финансы
```

- `KafkaSaleProducer` отправляет события в Kafka
- `SaleFeedConsumer` и `FinanceConsumer` читают их (фоновые сервисы)
- `SalesFeedService` и `FinanceService` хранят состояние для UI и уведомляют страницу об изменениях

Настройки Kafka (адрес, топик, id групп) лежат в `appsettings.json`, секция `Kafka`.

## Форматирование

В репозитории есть pre-commit хук с `dotnet format`. Чтобы включить его в своём клоне:

```
git config core.hooksPath .githooks
```

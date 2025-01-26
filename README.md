# Kafka-based Order Analytics

Этот репозиторий содержит pet-проект, в котором реализуется учёт проданного товара, а также (при желании) аналитика по продаваемому товару с использованием Apache Kafka. В качестве источника событий выступает сервис заказов (в нашем случае — консольное приложение, генерирующее сообщения в топик Kafka).

---

## Содержание

1. [Описание проекта](#описание-проекта)
2. [Формат входящих событий](#формат-входящих-событий)
3. [Основная логика сервиса](#основная-логика-сервиса)
4. [Дополнительные возможности](#дополнительные-возможности)
5. [Docker cheat sheet](#docker-cheat-sheet)
6. [Kafka cheat sheet](#kafka-cheat-sheet)
7. [FAQ](#faq)

---

## Описание проекта

В рамках данного проекта консольное приложение (или любой другой сервис заказов) публикует в топик Kafka события при создании заказа. Наш сервис читает эти события, обрабатывает их и сохраняет итоговые данные в базу (учёт проданных, отменённых, зарезервированных товаров).

Основная задача — реализовать витрину данных (или небольшую аналитику), которая будет учитывать:

- Сколько товаров в статусе `created` (зарезервированы)
- Сколько товаров в статусе `delivered` (продано)
- Сколько товаров в статусе `canceled` (отменено)
- Дату и время последнего обновления информации по каждому товару

---

## Формат входящих событий

При создании заказа сервис-источник (консольное приложение) формирует JSON-событие такого вида:

```json
{
    "moment": "timestamp",
    "order_id": "long",
    "user_id": "long",
    "warehouse_id": "long",
    "positions": {
        "item_id": "long",
        "quantity": "int",
        "price": {
            "currency": "RUR|KZT",
            "units": "long",
            "nanos": "int"
        }
    },
    "status": "Created|Canceled|Delivered"
}
```

### Детали контракта

1. **Первое событие по заказу** всегда будет со статусом `Created`.
2. **Второе событие** для этого заказа — `Canceled` или `Delivered`. Может и вовсе отсутствовать, если заказ не изменял статус.
3. **price** повторяет структуру [google.type.Money](https://cloud.google.com/apis/design/design_patterns#money_fields), где:
    - `currency` — валюта (`RUR` или `KZT`)
    - `units` — целая часть
    - `nanos` — дробная часть в диапазоне `0..1`, умноженная на 1 000 000 000  
      Пример: цена 5316.78 руб. будет `units=5316`, `nanos=780000000`.

---

## Основная логика сервиса

Сервис читает топик Kafka, парсит события и сохраняет данные в таблицу базы данных. В таблице рекомендуется хранить:
- `item_id`
- Количество зарезервированных (статус `created`)
- Количество проданных (переход `created → delivered`)
- Количество отменённых (переход `created → canceled`)
- Дату и время последнего обновления этих значений

Механика учёта:
- Если приходит первое событие (Created), то увеличивается счётчик «зарезервировано».
- Если приходит второе событие (Canceled или Delivered), то происходит перераспределение: счётчик «зарезервировано» уменьшается, а счётчик «продано» или «отменено» — увеличивается.

---

## Дополнительные возможности

Ниже перечислены идеи для расширения проекта:

1. **Учёт денежных средств:**
    - Поскольку `item_id` состоит из 12 цифр, где первые 6 — идентификатор продавца, а последние 6 — идентификатор товара, можно вести учёт продаж по каждому товару с разбивкой по валютам.
    - При этом важно корректно суммировать значения в разных валютах.

2. **Обработка concurrency и партиций:**
    - При запуске 2+ инстансов сервиса и наличии 2+ партиций в Kafka стоит уделить внимание тому, чтобы не возникали коллизии при подсчёте или при обработке сообщений несколькими экземплярами одновременно.

---

## Docker cheat sheet

- **Запуск docker-контейнеров (БД и т. п.):**
  ```bash
  docker compose up -d
  ```
- **Остановка контейнеров:**
  ```bash
  docker compose down
  ```
- **Остановка и очистка данных:**
  ```bash
  docker compose down -v
  ```
- **Если всё сломалось:**
  ```bash
  docker system prune
  ```

---

## Kafka cheat sheet

- Поднимаем Kafka через docker (пример docker-compose в этом репозитории).
- Для Windows нужно добавить в `c:\windows\system32\drivers\etc\hosts` строку:
  ```
  127.0.0.1 kafka
  ```
- **Offset Explorer** (ранее Kafka Tool) <https://www.kafkatool.com/> — простой UI для чтения/записи в Kafka.
    - Для protobuf-сообщений может быть непросто, но для JSON подойдёт.
    - Настройка:
        1. Clusters → Add New Connection
        2. Cluster Name → любое имя
        3. Вкладка Advanced → Bootstrap Servers → `kafka:9092`
        4. Нажать Test, затем Add
- **Альтернативно**, можно скачать Kafka напрямую [с официального сайта](https://kafka.apache.org/downloads) и использовать `.sh` / `.bat` скрипты для работы с локальной/докер/стейдж/прод-средой.

---

## FAQ

**Q: Я нашёл(ла) баг в генераторе событий. Что делать?**  
A: Мы все люди, и такое случается. Оставьте ишью в репозитории или сообщите об этом любым удобным способом.

**Q: Как организовать чтение в сервисе?**  
A: Используйте [Background Hosted Service](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-7.0&tabs=visual-studio), который будет постоянно слушать топик Kafka и обрабатывать приходящие сообщения.

---

**Приятной разработки!**

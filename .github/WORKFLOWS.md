<!--
SPDX-FileCopyrightText: 2026 Yaroslav Yudaev <ydaevy10@gmail.com>

SPDX-License-Identifier: MIT
-->

# GitHub Конфигурация

В этой директории лежат шаблоны issue и PR, `CODEOWNERS`, настройки меток и GitHub Actions workflow.

## Активные Workflow

### CI и валидация

- `build-map-renderer.yml`
  - Собирает и запускает `Content.MapRenderer`.
  - Триггеры: `push`, `pull_request`, `merge_group`.

- `build-test-debug.yml`
  - Основной debug CI.
  - Собирает проект и запускает `Content.Tests` и `Content.IntegrationTests`.
  - Триггеры: `push`, `pull_request`, `merge_group`.

- `check-crlf.yml`
  - Проверяет PR на CRLF.
  - Триггеры: `pull_request`.

- `no-submodule-update.yml`
  - Падает, если PR меняет указатель submodule `RobustToolbox`.
  - Триггеры: `pull_request` при изменениях по пути `RobustToolbox`.

- `test-packaging.yml`
  - Собирает `Content.Packaging` и проверяет упаковку server/client.
  - Триггеры: `push`, `pull_request`, `merge_group`.

- `validate-rgas.yml`
  - Валидирует RGA YAML по схеме и кастомным валидаторам.
  - Триггеры: `push`, `pull_request`, `merge_group`.

- `validate-rsis.yml`
  - Валидирует RSI-ресурсы.
  - Триггеры: `push`, `pull_request`, `merge_group`.

- `validate_mapfiles.yml`
  - Валидирует map YAML по схеме и кастомным валидаторам.
  - Триггеры: `push`, `pull_request`, `merge_group`.

- `yaml-linter.yml`
  - Собирает и запускает YAML linter.
  - Триггеры: `push`, `pull_request`, `merge_group`.

### Автоматизация PR и меток

Все workflow из этого раздела работают на встроенном `github.token` c явно заданными `permissions`.
Отдельные PAT или кастомные bot-token secrets для них не нужны.
Для workflow, которые ставят метки или пишут комментарии в PR, нужны оба права: `issues: write` и `pull-requests: write`.

- `check-author-repoban.yml`
  - Закрывает PR от soft-banned author ID и оставляет комментарий.
  - Триггеры: `pull_request_target`.

- `close-master-pr.yml`
  - Закрывает PR, если он открыт из `master`, `main` или `develop`.
  - Триггеры: `pull_request_target`.

- `labeler-conflict.yml`
  - Ставит `S: Merge Conflict` и пишет комментарий при merge conflicts.
  - Триггеры: `pull_request_target`.

- `labeler-needsreview.yml`
  - Ставит `S: Needs Review` при запросе review и снимает `S: Awaiting Changes`.
  - Триггеры: `pull_request_target`.

- `labeler-pr.yml`
  - Ставит и синхронизирует только path-based метки `Changes:*`.
  - Не трогает чужие метки вроде `size/*`, `S:*`, branch labels и т.д.
  - Триггеры: `pull_request_target`.

- `labeler-review.yml`
  - Обновляет review-метки для внутренних PR.
  - При approve ставит `S: Approved`.
  - При `changes_requested` ставит `S: Awaiting Changes`.
  - Триггеры: `pull_request_review`.
  - Для PR из форков запись меток на этом событии intentionally не используется, потому что GitHub даёт read-only token.

- `labeler-size.yml`
  - Ставит размерную метку PR (`XS`, `S`, `M`, `L`, `XL`) по размеру diff.
  - Триггеры: `pull_request_target`.

- `labeler-stable.yml`
  - Ставит `Branch: Stable` для PR в `stable`.
  - Триггеры: `pull_request_target`.

- `labeler-staging.yml`
  - Ставит `Branch: Staging` для PR в `staging`.
  - Триггеры: `pull_request_target`.

- `labeler-untriaged.yml`
  - Ставит `S: Untriaged` новым issue и PR без меток.
  - Триггеры: `issues`, `pull_request_target`.

- `rsi-diff.yml`
  - Генерирует diff по RSI-картинкам и создаёт или обновляет комментарий в PR.
  - Триггеры: `pull_request_target` при изменениях `.rsi/*.png`.

- `reuse-updater.yml`
  - Проверяет изменённые `.cs`, `.yml` и `.yaml` файлы на отсутствие REUSE/SPDX headers.
  - Запускает доверенный скрипт из base branch против checkout PR.
  - Ничего не пушит обратно.
  - Создаёт patch artifact и оставляет или обновляет комментарий с предложенными правками.
  - Триггеры: `pull_request_target`.

- `stale.yml`
  - Помечает stale PR и затем закрывает их по таймауту.
  - Триггеры: `workflow_dispatch`, `schedule`.

### Релизы и обслуживание

- `changelog.yml`
  - После merge PR парсит changelog-блок из тела PR и дописывает его в Goob changelog YAML.
  - Коммитит обновлённый changelog обратно в целевую ветку.
  - Триггеры: `pull_request_target` на `closed`.

- `publish.yml`
  - Ручной release workflow для `development` или `production`.
  - Собирает и пакует проект, публикует сборку в CDN и отправляет changelog в Discord.
  - Берёт `ROBUST_CDN_URL` и `FORK_ID` из GitHub Environment variables у выбранного environment.
  - Триггеры: `workflow_dispatch`.

- `update-credits.yml`
  - Периодически обновляет credits contributors и создаёт PR.
  - Триггеры: `workflow_dispatch`, `schedule`.

- `upstream.yml`
  - Еженедельный или ручной sync upstream в PR против ветки `rebase`.
  - Триггеры: `workflow_dispatch`, `schedule`.

## Отключённые Workflow

- `workflows-disabled/benchmarks.yml.disabled`
  - Отключённый benchmark workflow.

- `workflows-disabled/publish_changelog_rss.yml.disabled`
  - Отключённый workflow публикации changelog RSS.

## Справочник По env / vars / secrets

Ниже перечислены все переменные окружения, GitHub Variables и Secrets, которые сейчас используются в workflow или ожидаются ими.

### Built-in GitHub token

- `github.token` / `secrets.GITHUB_TOKEN`
  - Тип: встроенный токен GitHub Actions.
  - Где используется: разные workflow для GitHub API, комментариев, label-операций, checkout и push.
  - Нужно заводить вручную: нет.
  - Обязателен: нет, GitHub выдаёт его автоматически.
  - Значение по умолчанию: выдаётся GitHub автоматически для каждого workflow run.

### Repo vars

- `CHANGELOG_USER`
  - Тип: GitHub Actions variable.
  - Где используется: `changelog.yml`.
  - Назначение: имя автора для автоматического git commit changelog.
  - Обязателен: да, если используется `changelog.yml`.
  - Значение по умолчанию: нет.

- `CHANGELOG_EMAIL`
  - Тип: GitHub Actions variable.
  - Где используется: `changelog.yml`.
  - Назначение: email автора для автоматического git commit changelog.
  - Обязателен: да, если используется `changelog.yml`.
  - Значение по умолчанию: нет.

- `CHANGELOG_MESSAGE`
  - Тип: GitHub Actions variable.
  - Где используется: `changelog.yml`.
  - Назначение: шаблон commit message для changelog-коммита.
  - Обязателен: да, если используется `changelog.yml`.
  - Значение по умолчанию: нет.

- `CHANGELOG_BRANCH`
  - Тип: GitHub Actions variable.
  - Где используется: `changelog.yml`.
  - Назначение: ветка, в которую коммитится changelog.
  - Обязателен: нет.
  - Значение по умолчанию: `master`.

- `CHANGELOG_DIR`
  - Тип: GitHub Actions variable.
  - Где используется: `changelog.yml`.
  - Назначение: путь до changelog YAML файла.
  - Обязателен: нет.
  - Значение по умолчанию: `Resources/Changelog/GoobChangelog.yml`.

### Active secrets

- `PUBLISH_TOKEN`
  - Тип: GitHub Actions secret.
  - Где используется: `publish.yml`.
  - Назначение: bearer token для публикации сборки в CDN/API publish endpoint.
  - Обязателен: да, если используется `publish.yml`.
  - Значение по умолчанию: нет.

- `CHANGELOG_DISCORD_WEBHOOK`
  - Тип: GitHub Actions secret.
  - Где используется: `publish.yml`.
  - Назначение: Discord webhook для отправки changelog после publish.
  - Обязателен: да, если используется Discord changelog publish шаг.
  - Значение по умолчанию: нет.

### GitHub environments

- `development`
  - Тип: GitHub Environment.
  - Где используется: `publish.yml`.
  - Назначение: среда для development publish.
  - Обязателен: да, если запускается `publish.yml` с target `development`.
  - Значение по умолчанию: нет.

- `production`
  - Тип: GitHub Environment.
  - Где используется: `publish.yml`.
  - Назначение: среда для production publish.
  - Обязателен: да, если запускается `publish.yml` с target `production`.
  - Значение по умолчанию: нет.

### GitHub Environment variables

Это GitHub Actions Variables, которые должны быть заведены внутри environment `development` и `production`.
В `publish.yml` они прокидываются в обычные `env` переменные процесса перед запуском `Tools/publish_multi_request.py`.

- `ROBUST_CDN_URL`
  - Тип: GitHub Actions variable на уровне environment.
  - Где используется: `publish.yml` через `Tools/publish_multi_request.py`.
  - Назначение: базовый URL CDN/API для публикации сборок.
  - Обязателен: да, если используется `publish.yml`.
  - Значение по умолчанию: нет.

- `FORK_ID`
  - Тип: GitHub Actions variable на уровне environment.
  - Где используется: `publish.yml` через `Tools/publish_multi_request.py`.
  - Назначение: идентификатор форка в CDN/API publish endpoint.
  - Обязателен: да, если используется `publish.yml`.
  - Значение по умолчанию: нет.

### Secrets и env для отключённых workflow

Это значения, которые сейчас не нужны активным workflow, потому что соответствующие workflow отключены.

- `CENTCOMM_ROBUST_BENCHMARK_RUNNER_KEY`
  - Тип: GitHub Actions secret.
  - Где использовался: `workflows-disabled/benchmarks.yml.disabled`.
  - Назначение: SSH-ключ для удалённого benchmark runner.
  - Обязателен сейчас: нет.
  - Значение по умолчанию: нет.

- `BENCHMARKS_WRITE_ADDRESS`
  - Тип: GitHub Actions secret.
  - Где использовался: `workflows-disabled/benchmarks.yml.disabled`.
  - Назначение: адрес БД для записи benchmark результатов.
  - Обязателен сейчас: нет.
  - Значение по умолчанию: нет.

- `BENCHMARKS_WRITE_PORT`
  - Тип: GitHub Actions secret.
  - Где использовался: `workflows-disabled/benchmarks.yml.disabled`.
  - Назначение: порт БД benchmark результатов.
  - Обязателен сейчас: нет.
  - Значение по умолчанию: нет.

- `BENCHMARKS_WRITE_USER`
  - Тип: GitHub Actions secret.
  - Где использовался: `workflows-disabled/benchmarks.yml.disabled`.
  - Назначение: пользователь БД benchmark результатов.
  - Обязателен сейчас: нет.
  - Значение по умолчанию: нет.

- `BENCHMARKS_WRITE_PASSWORD`
  - Тип: GitHub Actions secret.
  - Где использовался: `workflows-disabled/benchmarks.yml.disabled`.
  - Назначение: пароль БД benchmark результатов.
  - Обязателен сейчас: нет.
  - Значение по умолчанию: нет.

- `CHANGELOG_RSS_KEY`
  - Тип: GitHub Actions secret.
  - Где использовался: `workflows-disabled/publish_changelog_rss.yml.disabled`.
  - Назначение: SSH private key для публикации changelog RSS на удалённый сервер.
  - Обязателен сейчас: нет.
  - Значение по умолчанию: нет.

## Synopsis

Данный проект создан в качестве тестового задания для трудоустройства в компанию

## Usage

Запустить ConsoleApp1.exe, ввести dbinfoset host port sid user password, чтобы установить параметры для подключения к Oracle DB, do <полный путь к папке> для обработки всех файлов и файлов подкаталогов указанной папки и записи их в БД, при этом автоматически создается таблица FileInfo с двумия полями - полным путем к файлу и его MD5 хеш-суммой с символами верхнего регистра. Если файл уже был ранее обработан, его хеш-сумма будет перезаписана.

## Installation

Скопировать Oracle.ManagedDataAccess.dll и ConsoleApp1.exe и положить в одно место. Oracle DB Server

## Tests

Успешным выполнением задания будет являться совпадение количества обработанных файлов с количеством вставленных в БД, при этом должна вывестись информация о времени, затраченном на исполнении задания, в случае какой-либо ошибки сценарий прерывается и в консоли выводится причина его прерывания с возможностью продолжить работу.

## License

GNU GPL v2.

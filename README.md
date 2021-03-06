# Test-task-Asynchronous-sockets
> Напишите небольшую программу на языке C++ или C#, которая будет
> подключаться по этому же адресу/порту, но вместо посылания
> "Greetings<ПЕРЕВОД СТРОКИ>", она посылает целое число от 1 до 2018
> в текстовом представлении, тоже с переводом строки в конце.
> В ответ на посланное число сервер ответит другим целым числом
> (0 <= x < 1e7) - строкой с символом line feed в конце и любым
> количеством пробельных символов или точек ('.') слева и справа
> от числа.
>
> В итоге получается, что таким образом можно получить 2018 целых
> чисел. На каждое входное число всегда выдается ответом одно и то
> же выходное число, то есть их можно запрашивать в любом порядке,
> и, если нужно, по несколько раз.
>
> Нужно рассчитать медиану этих 2018 числовых значений.
> А ещё сложность в том, что сервер специально задерживает ответ,
> поэтому по очереди получить все числа займёт очень много времени.
> Нужно, чтобы программа была многопоточная, и делала запросы
> параллельно. Учтите в работе программы, что под нагрузкой
> подключение по сети может быть не всегда доступно или не всегда
> стабильно, или сервер может упасть или дропнуть соединение
> под нагрузкой.
>
> Ответ пришлите в виде числа (рассчитанного вещественного значения медианы).

# Базовые сокращения Zen Coding

- Тег:
div → <div></div>

- ID:
div#header → <div id="header"></div>

- Класс:
div.menu → <div class="menu"></div>

- Несколько классов:
div.menu.main → <div class="menu main"></div>

- Атрибуты:
a[title="Zen Coding"] → <a title="Zen Coding"></a>

- Структурные операторы
Дочерний элемент (>)
ul>li →
<ul>
  <li></li>
</ul>

- Соседний элемент (+)
h1+p →
<h1></h1>
<p></p>

- Умножение (*)
li*3 →
<li></li>
<li></li>
<li></li>

- Группировка (())
div>(header>ul>li*2)+footer>p →
<div>
  <header>
    <ul>
      <li></li>
      <li></li>
    </ul>
  </header>
  <footer>
    <p></p>
  </footer>
</div>

- Нумерация элементов ($)
ul>li.item$*3 →
<ul>
  <li class="item1"></li>
  <li class="item2"></li>
  <li class="item3"></li>
</ul>


# Сниппеты (шорткоды)

- a → <a href=""></a>

- img → <img src="" alt="" />

- input:password → <input type="password" name="" id="" />

- form:get → <form action="" method="get"></form>

- link:css → <link rel="stylesheet" href="style.css">

- script:src → <script src=""></script>


# Дополнительные возможности

- Текст внутри тега
p{Hello Zen} → <p>Hello Zen</p>

- Оборачивание выделенного текста
(Wrap with Abbreviation)

- Инкремент/декремент числа
(Increment/Decrement number by 1)

- Удаление тега
(Remove Tag)


# CSS-сокращения

- Свойства CSS
m10 → margin: 10px;
p10-20 → padding: 10px 20px;
bgc → background-color: ;


# Примеры сложных сокращений

- div#header>ul#nav>li*4>a
<div id="header">
  <ul id="nav">
    <li><a href=""></a></li>
    <li><a href=""></a></li>
    <li><a href=""></a></li>
    <li><a href=""></a></li>
  </ul>
</div>

- table>tr*2>td*3
<table>
  <tr>
    <td></td>
    <td></td>
    <td></td>
  </tr>
  <tr>
    <td></td>
    <td></td>
    <td></td>
  </tr>
</table>
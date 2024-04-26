@import 'src/vars.scss';

table {
  border-collapse: collapse;
  border-radius: 5px;
}

table td {
  padding: 8px;
}

table th {
  padding: 8px;
}

@media (prefers-color-scheme: dark) {
  table td {
    border: 1px solid $dark-text-light-color;
  }

}

@media (prefers-color-scheme: light) {
  table td {
    border: 1px solid $light-text-light-color;
  }

}
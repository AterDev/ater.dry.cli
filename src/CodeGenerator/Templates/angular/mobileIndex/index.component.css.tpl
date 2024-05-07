@import 'src/vars.scss';

mat-list-item {
  height: auto !important;
  margin: 4px 0;
  padding: 2px 8px;
}

@media (prefers-color-scheme: dark) {
  mat-list-item {
    border: 1px solid $dark-bgh-color;
  }
}

@media (prefers-color-scheme: light) {
  mat-list-item {
    border: 1px solid $light-bgh-color;
  }
}

mat-form-field {
  width: auto;
}
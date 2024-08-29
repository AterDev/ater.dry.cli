@import 'src/vars.scss';

.top-bar {
  width: 100%;
  height: 56px;
  flex-shrink: 0;
}

.list-container {
  flex: 1;
  padding: 0 8px;
  overflow-y: auto;
}

mat-list-item {
  height: auto !important;
  margin-bottom: 8px;
  padding: 4px 8px;
}

@media (prefers-color-scheme: dark) {
  mat-list-item {
    border: 1px solid lighten($color: $dark-bgh-color, $amount: 5%);
  }
}

@media (prefers-color-scheme: light) {
  mat-list-item {
    border: 1px solid darken($color: $light-bgh-color, $amount: 5%);
  }
}

mat-form-field {
  width: auto;
}
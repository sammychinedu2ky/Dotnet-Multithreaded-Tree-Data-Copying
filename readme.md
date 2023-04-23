This code tries to copy a tree from one tree to another via multithreading without to improve performance. The copying of data is executed from bottom to top.
```
graph TD
  A --> B
  A --> C
  C --> D
  C --> E
  E --> F
  E --> G
  G --> H
  G --> I
  D --> Sam
  D --> Cat
```

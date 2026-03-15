# Extermist Detector

Simple microservices system for detect "prohibited" content

## How it works

- **Informer** - sends everything he gets to the inquisitors
- **Inquisitor** - reviews requests for prohibited content, notifies the "butcher" about the discovered apostate
- **Butcher** - puts all apostates on public display

## How to start
```dockerfile
    docker compose up --build
    ## If you run from vs or rider DISABLE FAST MODE
    ## Inquisitor needed custom paths for tesseract
```


## Ideology of project

- **First make it work** - simple working module at first - normal standardize code later
- **Data loss is acceptable** - losing 5-10% of data isn't as bad as slowing
- **Tests** - Tests is good, but first, MAKE IT WORK
- **Logging** - Logging shouldn't affect to performance
# Diagrama de Classes do Dominio

O modelo separa os contextos de Estoque e Faturamento. `StockMovement` e `PrintOperation` existem para garantir auditoria, recuperacao de falhas e idempotencia no fluxo de impressao.

```mermaid
classDiagram
    class Product {
        +UUID id
        +string code
        +string description
        +decimal availableQuantity
        +byte[] rowVersion
        +decreaseStock(quantity)
    }

    class StockMovement {
        +UUID id
        +UUID productId
        +string operationKey
        +decimal quantity
        +datetime createdAt
    }

    class Invoice {
        +UUID id
        +long number
        +InvoiceStatus status
        +datetime createdAt
        +addItem(productId, description, quantity)
        +close()
    }

    class InvoiceItem {
        +UUID id
        +UUID productId
        +string productCode
        +string productDescription
        +decimal quantity
    }

    class PrintOperation {
        +UUID id
        +UUID invoiceId
        +string idempotencyKey
        +PrintOperationStatus status
        +int attempts
        +string errorCode
        +datetime createdAt
        +datetime completedAt
        +complete()
        +fail(errorCode)
    }

    class InvoiceStatus {
        <<enumeration>>
        Open
        Closed
    }

    class PrintOperationStatus {
        <<enumeration>>
        Pending
        Failed
        Completed
    }

    Product "1" --> "0..*" StockMovement : registra baixas
    Invoice "1" *-- "1..*" InvoiceItem : possui
    Invoice "1" --> "0..*" PrintOperation : historico
    Invoice --> InvoiceStatus
    PrintOperation --> PrintOperationStatus
    InvoiceItem ..> Product : referencia por productId
```

## Fronteiras de persistencia

| Microsservico | Classes persistidas |
|---|---|
| Inventory Service | `Product`, `StockMovement` |
| Billing Service | `Invoice`, `InvoiceItem`, `PrintOperation` |

`InvoiceItem` guarda codigo e descricao como snapshot para que a nota mantenha seu historico mesmo se o produto for alterado posteriormente.

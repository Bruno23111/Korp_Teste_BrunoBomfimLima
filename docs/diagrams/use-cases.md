# Diagrama de Casos de Uso

Este diagrama define o limite funcional da primeira versao do sistema de emissao de notas fiscais. O ator `Operador` representa o usuario interno responsavel por cadastro, faturamento e impressao.

```mermaid
flowchart LR
    operator["Operador"]

    subgraph invoiceSystem["Sistema de Emissao de Notas Fiscais"]
        manageProducts(["Cadastrar e consultar produtos"])
        manageInvoice(["Criar nota fiscal"])
        addItems(["Incluir varios produtos e quantidades"])
        viewInvoice(["Consultar nota fiscal"])
        printInvoice(["Imprimir nota fiscal"])
        showProcessing(["Exibir processamento"])
        updateStock(["Baixar estoque"])
        closeInvoice(["Fechar nota fiscal"])
        handleFailure(["Informar falha e permitir nova tentativa"])
    end

    operator --> manageProducts
    operator --> manageInvoice
    operator --> viewInvoice
    operator --> printInvoice

    manageInvoice -. inclui .-> addItems
    printInvoice -. inclui .-> showProcessing
    printInvoice -. inclui .-> updateStock
    printInvoice -. inclui .-> closeInvoice
    printInvoice -. em caso de falha .-> handleFailure
```

## Regras representadas

- A nota nasce com status `Open` e deve conter ao menos um item.
- A impressao so e permitida para nota aberta.
- A baixa de estoque e o fechamento so acontecem quando a operacao de impressao termina com sucesso.
- Falha no servico de estoque preserva a nota aberta, mostra feedback ao operador e permite uma repeticao idempotente.


import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Product { id: string; code: string; description: string; availableQuantity: number; }
interface Invoice { id: string; number: number; status: number; }

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  products: Product[] = [];
  invoices: Invoice[] = [];
  loading = false;
  message = '';
  error = '';
  product = { code: '', description: '', availableQuantity: 0 };
  invoice = { productId: '', quantity: 1 };

  constructor(private readonly http: HttpClient) {}

  ngOnInit(): void { this.refresh(); }

  refresh(): void {
    this.loading = true;
    this.error = '';
    this.http.get<Product[]>('http://localhost:5219/api/products').subscribe({
      next: products => this.http.get<Invoice[]>('http://localhost:5290/api/invoices').subscribe({
        next: invoices => { this.products = products; this.invoices = invoices; this.loading = false; },
        error: error => this.fail(error)
      }),
      error: error => this.fail(error)
    });
  }

  createProduct(): void {
    this.http.post('http://localhost:5219/api/products', this.product).subscribe({
      next: () => { this.message = 'Produto cadastrado com sucesso.'; this.product = { code: '', description: '', availableQuantity: 0 }; this.refresh(); },
      error: error => this.fail(error)
    });
  }

  createInvoice(): void {
    const product = this.products.find(item => item.id === this.invoice.productId);
    if (!product) { this.error = 'Selecione um produto para a nota.'; return; }

    const request = { items: [{ productId: product.id, productCode: product.code, productDescription: product.description, quantity: this.invoice.quantity }] };
    this.http.post('http://localhost:5290/api/invoices', request).subscribe({
      next: () => { this.message = 'Nota fiscal criada com sucesso.'; this.refresh(); },
      error: error => this.fail(error)
    });
  }

  print(invoice: Invoice): void {
    this.http.post(`http://localhost:5290/api/invoices/${invoice.id}/print`, { idempotencyKey: `ui-${invoice.id}` }).subscribe({
      next: () => { this.message = `Nota #${invoice.number} impressa e fechada.`; this.refresh(); },
      error: error => this.fail(error)
    });
  }

  private fail(error: any): void {
    this.loading = false;
    this.error = error?.error?.detail ?? 'Não foi possível concluir a operação. Confirme que as APIs estão em execução.';
  }
}

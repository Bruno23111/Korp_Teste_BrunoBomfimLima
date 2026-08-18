import { afterNextRender, ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { finalize, forkJoin, retry, timer } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { ChartModule } from 'primeng/chart';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TooltipModule } from 'primeng/tooltip';

type View =
  'dashboard' | 'products' | 'product-form' | 'product-edit' | 'invoices' | 'invoice-form';

interface Product {
  id: string;
  code: string;
  description: string;
  availableQuantity: number;
}

interface InvoiceItem {
  productId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

interface Invoice {
  id: string;
  number: number;
  status: number;
  items: InvoiceItem[];
  createdAt: string;
}

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    ChartModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    SelectModule,
    TableModule,
    TagModule,
    ToggleSwitchModule,
    TooltipModule,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private static readonly themeStorageKey = 'korp-theme-v2';
  view: View = 'dashboard';
  products: Product[] = [];
  invoices: Invoice[] = [];
  loading = false;
  message = '';
  error = '';
  private successTimeout?: ReturnType<typeof setTimeout>;
  darkMode = false;

  product = { code: '', description: '', availableQuantity: 0 };
  editingProductId: string | null = null;
  showDeleteConfirmation = false;
  invoiceItem = { productId: '', quantity: 1 };
  invoiceItems: InvoiceItem[] = [];

  constructor(
    private readonly http: HttpClient,
    private readonly changeDetector: ChangeDetectorRef,
  ) {
    afterNextRender(() => {
      this.darkMode = localStorage.getItem(App.themeStorageKey) === 'dark';
      this.applyTheme();
      this.refresh();
    });
  }

  get productsWithLowStock(): number {
    return this.products.filter((product) => product.availableQuantity <= 2).length;
  }

  get openInvoices(): number {
    return this.invoices.filter((invoice) => invoice.status === 1).length;
  }

  get stockChart(): Product[] {
    return [...this.products]
      .sort((left, right) => right.availableQuantity - left.availableQuantity)
      .slice(0, 6);
  }

  get maxStock(): number {
    return Math.max(...this.stockChart.map((product) => product.availableQuantity), 1);
  }

  get productOptions(): { label: string; value: string }[] {
    return this.products.map((product) => ({
      label: `${product.code} - ${product.description} (${product.availableQuantity})`,
      value: product.id,
    }));
  }

  get stockChartData() {
    return {
      labels: this.stockChart.map((product) => product.code),
      datasets: [
        {
          label: 'Saldo disponível',
          data: this.stockChart.map((product) => product.availableQuantity),
        backgroundColor: this.darkMode ? '#a78bfa' : '#7c3aed',
        hoverBackgroundColor: this.darkMode ? '#c4b5fd' : '#8b5cf6',
          borderRadius: 8,
          borderSkipped: false,
          maxBarThickness: 46,
        },
      ],
    };
  }

  get stockChartOptions() {
    const textColor = this.darkMode ? '#ddd6fe' : '#475569';
    const gridColor = this.darkMode ? 'rgba(221, 214, 254, .12)' : 'rgba(71, 85, 105, .12)';
    return {
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false }, ticks: { color: textColor } },
        y: {
          beginAtZero: true,
          grid: { color: gridColor },
          ticks: { color: textColor, precision: 0 },
        },
      },
    };
  }

  applyTheme(): void {
    document.documentElement.classList.toggle('app-dark', this.darkMode);
    localStorage.setItem(App.themeStorageKey, this.darkMode ? 'dark' : 'light');
  }

  get isProductFormValid(): boolean {
    return (
      this.product.code.trim().length > 0 &&
      this.product.code.trim().length <= 20 &&
      this.product.description.trim().length > 0 &&
      this.product.description.trim().length <= 100 &&
      Number.isFinite(this.product.availableQuantity) &&
      this.product.availableQuantity >= 0
    );
  }

  navigate(view: View): void {
    this.view = view;
    this.message = '';
    this.error = '';

    if (view === 'dashboard' || view === 'products' || view === 'invoices') {
      this.refresh();
    }
  }

  refresh(): void {
    this.loading = true;
    this.error = '';
    const cacheBust = `?_=${Date.now()}`;

    forkJoin({
      products: this.http.get<Product[]>(`http://localhost:5219/api/products${cacheBust}`),
      invoices: this.http.get<Invoice[]>(`http://localhost:5290/api/invoices${cacheBust}`),
    })
      .pipe(
        retry({ count: 3, delay: () => timer(1000) }),
        finalize(() => {
          this.loading = false;
          this.changeDetector.detectChanges();
        }),
      )
      .subscribe({
        next: ({ products, invoices }) => {
          this.products = products;
          this.invoices = invoices;
          this.changeDetector.detectChanges();
        },
        error: (error) => this.fail(error),
      });
  }

  createProduct(): void {
    if (!this.isProductFormValid) {
      this.error = 'Preencha os campos obrigatórios com valores válidos.';
      return;
    }

    this.http.post<Product>('http://localhost:5219/api/products', this.product).subscribe({
      next: (product) => {
        this.products = [...this.products, product].sort((left, right) =>
          left.code.localeCompare(right.code),
        );
        this.product = { code: '', description: '', availableQuantity: 0 };
        this.navigate('products');
        this.showSuccess('Produto cadastrado com sucesso.');
        this.changeDetector.detectChanges();
      },
      error: (error) => this.fail(error),
    });
  }

  openNewProduct(): void {
    this.product = { code: '', description: '', availableQuantity: 0 };
    this.editingProductId = null;
    this.navigate('product-form');
  }

  editProduct(product: Product): void {
    this.editingProductId = product.id;
    this.product = {
      code: product.code,
      description: product.description,
      availableQuantity: product.availableQuantity,
    };
    this.navigate('product-edit');
  }

  updateProduct(): void {
    if (!this.editingProductId || !this.isProductFormValid) {
      this.error = 'Preencha os campos obrigatórios com valores válidos.';
      return;
    }

    this.http
      .put<Product>(`http://localhost:5219/api/products/${this.editingProductId}`, this.product)
      .subscribe({
        next: (product) => {
          this.products = this.products
            .map((item) => (item.id === product.id ? product : item))
            .sort((left, right) => left.code.localeCompare(right.code));
          this.navigate('products');
          this.showSuccess('Produto atualizado com sucesso.');
          this.changeDetector.detectChanges();
        },
        error: (error) => this.fail(error),
      });
  }

  deleteProduct(): void {
    if (!this.editingProductId) {
      return;
    }

    this.showDeleteConfirmation = true;
  }

  cancelDeleteProduct(): void {
    this.showDeleteConfirmation = false;
  }

  confirmDeleteProduct(): void {
    if (!this.editingProductId) {
      return;
    }

    this.http.delete(`http://localhost:5219/api/products/${this.editingProductId}`).subscribe({
      next: () => {
        this.showDeleteConfirmation = false;
        this.products = this.products.filter((product) => product.id !== this.editingProductId);
        this.navigate('products');
        this.showSuccess('Produto excluído com sucesso.');
        this.changeDetector.detectChanges();
      },
      error: (error) => this.fail(error),
    });
  }

  addInvoiceItem(): void {
    const product = this.products.find((item) => item.id === this.invoiceItem.productId);
    if (!product) {
      this.error = 'Selecione um produto para adicionar à nota.';
      return;
    }

    if (this.invoiceItem.quantity <= 0) {
      this.error = 'A quantidade deve ser maior que zero.';
      return;
    }

    const existingItem = this.invoiceItems.find((item) => item.productId === product.id);
    if (existingItem) {
      existingItem.quantity += this.invoiceItem.quantity;
    } else {
      this.invoiceItems = [
        ...this.invoiceItems,
        {
          productId: product.id,
          productCode: product.code,
          productDescription: product.description,
          quantity: this.invoiceItem.quantity,
        },
      ];
    }

    this.invoiceItem = { productId: '', quantity: 1 };
    this.error = '';
  }

  removeInvoiceItem(productId: string): void {
    this.invoiceItems = this.invoiceItems.filter((item) => item.productId !== productId);
  }

  createInvoice(): void {
    if (this.invoiceItems.length === 0) {
      this.error = 'Adicione pelo menos um produto à nota.';
      return;
    }

    this.http
      .post<Invoice>('http://localhost:5290/api/invoices', { items: this.invoiceItems })
      .subscribe({
        next: (invoice) => {
          this.invoices = [invoice, ...this.invoices];
          this.invoiceItems = [];
          this.navigate('invoices');
          this.showSuccess(`Nota #${invoice.number} criada com sucesso.`);
          this.changeDetector.detectChanges();
        },
        error: (error) => this.fail(error),
      });
  }

  print(invoice: Invoice): void {
    this.http
      .post(`http://localhost:5290/api/invoices/${invoice.id}/print`, {
        idempotencyKey: `ui-${invoice.id}`,
      })
      .subscribe({
        next: () => {
          this.showSuccess(`Nota #${invoice.number} impressa e fechada.`);
          this.refresh();
        },
        error: (error) => this.fail(error),
      });
  }

  private fail(error: any): void {
    this.loading = false;
    const validationErrors = Object.values(error?.error?.errors ?? {})
      .flat()
      .join(' ');
    this.error =
      error?.error?.detail ||
      validationErrors ||
      error?.error?.title ||
      'Não foi possível concluir a operação. Confirme que as APIs estão em execução.';
    this.changeDetector.detectChanges();
  }

  private showSuccess(message: string): void {
    if (this.successTimeout) {
      clearTimeout(this.successTimeout);
    }

    this.message = message;
    this.error = '';
    this.successTimeout = setTimeout(() => {
      this.message = '';
      this.changeDetector.detectChanges();
    }, 5000);
  }
}

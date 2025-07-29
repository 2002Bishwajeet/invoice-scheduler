# InvoiceScheduler

InvoiceScheduler is an automated tool for generating and emailing monthly invoices as PDFs. It uses PuppeteerSharp to fill out an online invoicer form with your data, generates a PDF, and sends it to your client. The process can be run locally or automated via GitHub Actions for scheduled, hands-free invoicing.

> **Note:**  
> This project is intended for to be used with the Homebase projects only.

## Features

- Automated invoice PDF generation using PuppeteerSharp and a web invoicer
- Sends invoices via email automatically
- Updates invoice data for the next cycle
- Can be run locally or scheduled with GitHub Actions

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (for PuppeteerSharp to download Chromium)
- A `.env` file with your email and authentication details (see below)
- Homebase Identity - See [Homebase](https://homebase.id/) for more information

## Local Setup

### Homebase Identity

> Note: If you want to run the identities locally, you'll need the back-end web server. See the[Odin repository](https://github.com/homebase-id/odin-core) to get started.
>
> You don't need the full platform running locally as you can use a production identity during development of any app on ODIN.

This means you have two options for local development:

1. **Use Production Identity**: Connect to an existing Odin identity server for authentication and storage

2. **Full Local Setup**: Run the complete Odin platform locally (requires additional setup from the Odin repository)

3. **Clone the repository:**
   ```sh
   git clone https://github.com/2002Bishwajeet/InvoiceScheduler.git
   cd InvoiceScheduler
   ```
4. **Install dependencies:**
   ```sh
   dotnet restore InvoiceScheduler.sln
   ```
5. **Create your `.env` file:**
   Place a `.env` file in the `InvoiceScheduler/` directory with the following keys:
   ```env
   CLIENTAUTHTOKEN=your-client-auth-token
   SHAREDSECRET=your-shared-secret
   IDENTITY=your-identity
   RECIPIENT=recipient-identity
   ```
6. **Edit `data.json`:**
   Update the invoice and client details as needed (see sample below).

## Running Locally

From the root directory, run:

```sh
cd InvoiceScheduler
DOTNET_ENVIRONMENT=Development dotnet run
```

This will generate a PDF invoice in the `invoices/` folder and send it via email.

## GitHub Actions Automation

This project includes a workflow to generate and commit invoices automatically on the 1st of every month.

- Workflow file: `.github/workflows/monthly-invoice.yml`
- Triggers: Scheduled (monthly) and manual dispatch
- Steps:
  - Checks out the repo
  - Sets up .NET
  - Restores dependencies
  - Runs the invoice generator
  - Commits and pushes new invoices and updated `data.json`

**To enable automation:**

- Ensure your repository has the required secrets for email/auth (if needed)
- Push your `.env` and `data.json` files locally, but keep them in `.gitignore` to avoid leaking secrets

## Sample `data.json`

> **Note:**
>
> - This tool generates invoices using the following website: [invoicer.cloudx.run](https://invoicer.cloudx.run/)
> - For more details on the `data.json` structure and Puppeteer integration, see the [Puppeteer Integration section of the related GitHub README](https://github.com/2002Bishwajeet/serverless-functions/tree/main/functions/invoice_generator#setup-puppeteer-pdf-generation).

See [`data.example.json`](./InvoiceScheduler/data.example.json) for a complete sample `data.json` file.

## Usage Notes

- The invoice generator will skip creating a new invoice if one already exists for the current month.
- Make sure your `.env` and `data.json` are up to date before each run.
- All generated invoices are saved in the `invoices/` directory.

## License

This project is licensed under the terms of the MIT License. See the [LICENSE](LICENSE) file for details.

# InvoiceScheduler

InvoiceScheduler is an automated tool for generating and emailing monthly invoices as PDFs. It uses PuppeteerSharp to fill out an online invoicer form with your data, generates a PDF, and sends it to your client. The process can be run locally or automated via GitHub Actions for scheduled, hands-free invoicing.

> [!NOTE]
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

### Setting Up Repository Secrets for GitHub Actions

To run the workflow successfully, you must add the required environment variables as repository secrets in GitHub:

1. Go to your repository on GitHub.
2. Click on **Settings** > **Secrets and variables** > **Actions**.
3. Click **New repository secret** for each of the following keys:
   - `CLIENTAUTHTOKEN`
   - `SHAREDSECRET`
   - `IDENTITY`
   - `RECIPIENT`
4. Enter the value for each secret as required by your setup.

These secrets will be used by the GitHub Actions workflow to generate a `.env` file at runtime, ensuring your credentials and sensitive data are never exposed in the repository.

**To enable automation:**

- Ensure your repository has the required secrets for email/auth (see above)
- Push your `.env` and `data.json` files locally, but keep them in `.gitignore` to avoid leaking secrets

### Environment Variable Setup

All required environment variables, including `REPO_ROOT`, are automatically written to the `.env` file by the GitHub Actions workflow and should be present for local development as well. You do not need to set `REPO_ROOT` manually if you use the provided workflow or `.env` template.

**.env example:**

```
CLIENTAUTHTOKEN=...
SHAREDSECRET=...
IDENTITY=...
RECIPIENT=...
REPO_ROOT=/home/runner/work/invoice-scheduler-biswa/invoice-scheduler-biswa
```

Your code will use `REPO_ROOT` from the `.env` file to determine where invoices are saved, making the setup robust and portable for both local and CI runs.

---

## Sample `data.json`

> **Note:**
>
> - This tool generates invoices using the following website: [invoicer.cloudx.run](https://invoicer.cloudx.run/)
> - For more details on the `data.json` structure and Puppeteer integration, see the [Puppeteer Integration section of the related GitHub README](https://github.com/2002Bishwajeet/serverless-functions/tree/main/functions/invoice_generator#setup-puppeteer-pdf-generation).

See [`data.example.json`](./data.example.json) for a complete sample `data.json` file.

## Usage Notes

- The invoice generator will skip creating a new invoice if one already exists for the current month.
- Make sure your `.env` and `data.json` are up to date before each run.
- All generated invoices are saved in the `invoices/` directory.

## Invoice Save Location

Invoices are saved in the `InvoiceScheduler/invoices/` directory.

### Local Usage

By default, invoices are saved in `InvoiceScheduler/invoices/` relative to your current working directory. No configuration is needed for local runs.

### CI/Workflow Usage

In GitHub Actions, the workflow sets the `REPO_ROOT` environment variable to the repository root. This ensures invoices are saved in the correct location for automatic commit and push.

**Example workflow configuration:**

```yaml
env:
  REPO_ROOT: ${{ github.workspace }}
```

### Customization

If you want to override the save location, set the `REPO_ROOT` environment variable to your desired path before running the invoice generator.

---

## License

This project is licensed under the terms of the MIT License. See the [LICENSE](LICENSE) file for details.

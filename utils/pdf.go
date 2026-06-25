package utils

import (
	"bytes"
	"fmt"

	"github.com/dslipak/pdf"
)

// ExtractPDFText opens a PDF file and extracts all its text contents.
func ExtractPDFText(filePath string) (string, error) {
	r, err := pdf.Open(filePath)
	if err != nil {
		return "", fmt.Errorf("failed to open pdf: %w", err)
	}

	var buf bytes.Buffer
	totalPage := r.NumPage()
	for pageIndex := 1; pageIndex <= totalPage; pageIndex++ {
		p := r.Page(pageIndex)
		if p.V.IsNull() {
			continue
		}

		text, err := p.GetPlainText(nil)
		if err != nil {
			return "", fmt.Errorf("failed to get plain text from page %d: %w", pageIndex, err)
		}
		buf.WriteString(text)
		buf.WriteByte('\n')
	}

	return buf.String(), nil
}

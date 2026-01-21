package wanderer

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"time"

	"github.com/craig-jarvis/tripwire2wanderer/internal/config"
	"github.com/craig-jarvis/tripwire2wanderer/internal/data"
)

type Client struct {
	baseURL string
	mapSlug string
	apiKey  string
	client  *http.Client
}

// New creates a new Wanderer API client
func New(cfg *config.Config) *Client {
	return &Client{
		baseURL: cfg.WandererURL,
		mapSlug: cfg.WandererMapSlug,
		apiKey:  cfg.WandererAPIKey,
		client: &http.Client{
			Timeout: 30 * time.Second,
		},
	}
}

func (c *Client) DeleteSystemsAndConnections(request data.WandererSystemAndConnectionsDeleteRequest) error {
	url := fmt.Sprintf("%s/api/maps/%s/systems", c.baseURL, c.mapSlug)
	reqBody, err := json.Marshal(request)
	if err != nil {
		return fmt.Errorf("failed to marshal delete request: %w", err)
	}
	_, err = c.makeRequest("DELETE", url, reqBody)
	if err != nil {
		return err
	}

	return nil
}

// GetSystemsAndConnections retrieves systems and connections for a map from Wanderer
func (c *Client) GetSystemsAndConnections(ctx context.Context) (*data.WandererConnectionsAndSystemsEnvelope, error) {

	url := fmt.Sprintf("%s/api/maps/%s/systems", c.baseURL, c.mapSlug)
	body, err := c.makeRequest("GET", url, nil)
	if err != nil {
		return nil, err
	}

	var response data.WandererConnectionsAndSystemsEnvelope
	if err := json.Unmarshal(body, &response); err != nil {
		return nil, fmt.Errorf("failed to parse systems response: %w", err)
	}

	return &response, nil
}

func (c *Client) SubmitConnectionsAndSystems(request *data.WandererConnectionsAndSystemsEnvelope) (*data.WandererConnectionAndSystemCreateResponseEnvelope, error) {
	url := fmt.Sprintf("%s/api/maps/%s/systems", c.baseURL, c.mapSlug)
	reqBody, err := json.Marshal(request)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal request: %w", err)
	}
	body, err := c.makeRequest("POST", url, reqBody)
	if err != nil {
		return nil, err
	}

	var response data.WandererConnectionAndSystemCreateResponseEnvelope
	if err := json.Unmarshal(body, &response); err != nil {
		return nil, fmt.Errorf("failed to parse systems response: %w", err)
	}

	return &response, nil
}

// makeRequest makes an HTTP request with the specified verb, URL, and optional body with API key authentication
func (c *Client) makeRequest(verb string, url string, body []byte) ([]byte, error) {
	var bodyReader io.Reader
	if body != nil {
		bodyReader = bytes.NewReader(body)
	}
	req, err := http.NewRequest(verb, url, bodyReader)
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	// Set API key in header
	req.Header.Set("Authorization", fmt.Sprintf("Bearer %s", c.apiKey))
	req.Header.Set("Accept", "application/json")

	// Set Content-Type header if body is present
	if body != nil {
		req.Header.Set("Content-Type", "application/json")
	}

	resp, err := c.client.Do(req)
	if err != nil {
		return nil, fmt.Errorf("request failed: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		bodyBytes, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("API returned status %d: %s", resp.StatusCode, string(bodyBytes))
	}

	respBody, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response body: %w", err)
	}

	return respBody, nil
}

#!/usr/bin/env python3
"""Submit blog post URLs to Bing via the IndexNow API after each deployment."""

import json
import os
import sys
import urllib.request
import urllib.error
from html.parser import HTMLParser

ARCHIVES_URL = "https://christianhelle.com/archives"
INDEXNOW_ENDPOINT = "https://api.indexnow.org/indexnow"
HOST = "christianhelle.com"
API_KEY = os.environ.get("INDEXNOW_KEY", "93f380660c5f48eebdc7eb41b094762e")
KEY_LOCATION = f"https://{HOST}/{API_KEY}.txt"


class ArchivesLinkParser(HTMLParser):
    """Extract post URLs from the archives page."""

    def __init__(self):
        super().__init__()
        self.urls = []

    def handle_starttag(self, tag, attrs):
        if tag == "a":
            for attr, value in attrs:
                if attr == "href" and value and value.startswith(f"https://{HOST}/"):
                    if value not in self.urls:
                        self.urls.append(value)


def fetch_post_urls():
    print(f"Fetching archives from {ARCHIVES_URL} ...")
    with urllib.request.urlopen(ARCHIVES_URL, timeout=30) as response:
        html = response.read().decode("utf-8")
    parser = ArchivesLinkParser()
    parser.feed(html)
    print(f"Found {len(parser.urls)} URLs")
    return parser.urls


def submit_to_indexnow(urls):
    if not urls:
        print("No URLs to submit.")
        return

    payload = {
        "host": HOST,
        "key": API_KEY,
        "keyLocation": KEY_LOCATION,
        "urlList": urls,
    }
    body = json.dumps(payload).encode("utf-8")
    request = urllib.request.Request(
        INDEXNOW_ENDPOINT,
        data=body,
        headers={"Content-Type": "application/json; charset=utf-8"},
        method="POST",
    )

    print(f"Submitting {len(urls)} URLs to IndexNow ({INDEXNOW_ENDPOINT}) ...")
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            status = response.status
            print(f"Response status: {status}")
            if status in (200, 202):
                print("URLs submitted successfully.")
            else:
                print(f"Unexpected status code: {status}")
    except urllib.error.HTTPError as exc:
        print(f"HTTP error {exc.code}: {exc.reason}")
        sys.exit(1)
    except urllib.error.URLError as exc:
        print(f"URL error: {exc.reason}")
        sys.exit(1)


def main():
    urls = fetch_post_urls()
    submit_to_indexnow(urls)


if __name__ == "__main__":
    main()

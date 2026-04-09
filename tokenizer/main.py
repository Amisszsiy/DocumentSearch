from fastapi import FastAPI
from pydantic import BaseModel
from pythainlp.tokenize import word_tokenize

app = FastAPI()


class TokenizeRequest(BaseModel):
    text: str


class TokenizeResponse(BaseModel):
    result: str


class TokenizeBatchRequest(BaseModel):
    texts: list[str]


class TokenizeBatchResponse(BaseModel):
    results: list[str]


@app.post("/tokenize", response_model=TokenizeResponse)
def tokenize(request: TokenizeRequest):
    tokens = word_tokenize(request.text, engine="newmm", keep_whitespace=False)
    return TokenizeResponse(result=" ".join(tokens))


@app.post("/tokenize/batch", response_model=TokenizeBatchResponse)
def tokenize_batch(request: TokenizeBatchRequest):
    results = [
        " ".join(word_tokenize(text, engine="newmm", keep_whitespace=False))
        for text in request.texts
    ]
    return TokenizeBatchResponse(results=results)

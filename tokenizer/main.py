from fastapi import FastAPI
from pydantic import BaseModel
from pythainlp.tokenize import word_tokenize

app = FastAPI()


class TokenizeRequest(BaseModel):
    text: str


class TokenizeResponse(BaseModel):
    result: str


@app.post("/tokenize", response_model=TokenizeResponse)
def tokenize(request: TokenizeRequest):
    tokens = word_tokenize(request.text, engine="newmm", keep_whitespace=False)
    return TokenizeResponse(result=" ".join(tokens))

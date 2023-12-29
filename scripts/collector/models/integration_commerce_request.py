class Integration_Commerce_Request:
    def __init__(self, guid, index, total, commerce):
        self.guid = str(guid)
        self.index = index
        self.total = total
        self.commerce = commerce

    def to_dict(self):
        return {"guid": self.guid, "index": self.index, "total": self.total, "commerce": self.commerce.to_dict()}

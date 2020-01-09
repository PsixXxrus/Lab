import socket
import random
import sys

HEADER_NO_ID = "01 00 00 01 00 00 00 00 00 00"

A = 1
CNAME = 5
QCLASS = "00 01"

DNS_ADDRESS = "8.8.8.8"
DNS_PORT = 53
HEX_FORMAT = 16
MIN_POINTER_VALUE = 192


def generate_id() -> str:
    new_id = ""
    for i in range(4):
        new_id += str(random.randrange(0, 9))

    return new_id


def hex_string(num: int, padding: int) -> str:
    return "{0:0{1}x}".format(num, padding)


def normal_query(url: str) -> str:
    qname = ""
    for label in url.split("."):
        length = hex_string(len(label), 2)
        ascii_url = ""
        for char in label:
            ascii_url += hex_string(ord(char), 2)

        qname += length + ascii_url

    qname += "00"

    return qname


def compose_request(request_id: str, url: str, current_qtype: int) -> str:
    header = request_id + HEADER_NO_ID.replace(" ", "")

    qname = normal_query(url)

    qtype = hex_string(current_qtype, 4)

    query = qname + qtype + QCLASS.replace(" ", "")

    return header + query


def send_udp_message(message: str, dns_server: str) -> str:
    server_address = (dns_server, DNS_PORT)

    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        byte_message = bytes.fromhex(message)
        sock.sendto(byte_message, server_address)
        data, _ = sock.recvfrom(4096)
        result = data.hex()
        #print("Message: "+result)
    except socket.error:
        print("ошибка")
        sys.exit()
    finally:
        sock.close()

    return result


def check_response(response: str, actual_id: str) -> int:
    response_id = response[0:4]

    if response_id != actual_id:
        return -1

    num_answers = response[12:16]

    return int(num_answers, HEX_FORMAT)


def parse_response_ipv4(response: str, start_index: int) -> tuple:
    end_index = start_index + 4
    data_length = int(response[start_index:end_index], HEX_FORMAT)
    num_hex = 2 * data_length

    result = []
    for i in range(end_index, end_index + num_hex, 2):
        current_hex_string = response[i] + response[i + 1]
        result.append(str(int(current_hex_string, HEX_FORMAT)))

    string_result = ".".join(result)
    return string_result, num_hex


def follow_pointer(response: str, offset: int) -> str:
    result = []
    while True:
        current = response[offset:offset + 2]
        size = int(current, HEX_FORMAT)

        if size >= MIN_POINTER_VALUE:
            new_offset = int(response[offset + 2: offset + 4], HEX_FORMAT) * 2
            current_word = follow_pointer(response, new_offset)
            result.append(current_word)
            break

        elif size == 0:
            break

        offset += 2
        current_hex_string = response[offset:offset + (size * 2)]
        current_word = bytes.fromhex(current_hex_string).decode()
        result.append(current_word)

        offset += (size * 2)

    return ".".join(result)


def parse_response_canonical(
        response: str, response_only: str,start_index: int) -> tuple:
    end_index = start_index + 4
    data_length = int(response_only[start_index:end_index], HEX_FORMAT)
    num_hex = 2 * data_length

    result = []
    counter = num_hex
    index = end_index
    while counter > 0:    
        size = int(response_only[index:index + 2], HEX_FORMAT)

        if size >= MIN_POINTER_VALUE:
            offset = int(response_only[index + 2: index + 4], HEX_FORMAT) * 2
            current_word = follow_pointer(response, offset)
            result.append(current_word)
            print(current_word)
            break

        index += 2
        current_hex_string = response_only[index:index + (size * 2)]
        current_word = bytes.fromhex(current_hex_string).decode()
        result.append(current_word)
        print(current_word)

        index += (size * 2)
        counter -= 2 - (size * 2)
    
    return ".".join(result), num_hex


def get_type(message: str, start_index: int) -> int:
    type_index = start_index + 4
    qtype = message[type_index:type_index + 4]

    return int(qtype, HEX_FORMAT)


def process_request(url: str, request_type: int, dns_server: str) -> list:
    ips = []

    request_id = generate_id()

    message = compose_request(request_id, url, request_type)

    response = send_udp_message(message, dns_server)

    answers = check_response(response, request_id)
    if answers == -1:
        print("Что-то не так")

    response_only = response[len(message):]
    current_answer_index = 0
    length = 0
    for i in range(answers):
        answer_type = get_type(response_only, current_answer_index)
        if answer_type == A:
            data, length = parse_response_ipv4(
                response_only, current_answer_index + 20)
            ips.append(data)

        elif answer_type == CNAME:
            data, length = parse_response_canonical(
                response, response_only, current_answer_index + 20)
            if request_type == CNAME:
                ips.append(data)

        current_answer_index += (4 * 6) + length

    return ips


def dns_lookup(url: str, dns_server: str, reverse: bool) -> dict:
    result = {}

    if reverse:
        url = process_request(url, PTR, dns_server)[0]

    result["hostname"] = url
    result["ipv4"] = process_request(url, A, dns_server)
    result["canonical"] = process_request(url, CNAME, dns_server)

    return result


def main():

    print("Host name: " + url + "\n")
    ipv4 = process_request(url, A, DNS_ADDRESS)
    print("IPv4 Address(es):")
    for ip4 in ipv4:
        print(ip4)

    print("")

    print("Canonical Host Name")
    cname = process_request(url, CNAME, DNS_ADDRESS)
    for canonical in cname:
        print("")
        print(canonical)

if __name__ == "__main__":
    if len (sys.argv) > 1:
        url = sys.argv[1]
        main()
    else:
        url = "edu.zaikin.su"
        main()

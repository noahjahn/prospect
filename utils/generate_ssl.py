from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization, hashes
from cryptography.x509 import NameOID, CertificateBuilder
from cryptography import x509
import datetime
import os

def main():
    if os.path.exists("certificate.pfx"):
        q = input("certificate.pfx already exists. Are you sure you want to generate a new certificate? (y/n): ")
        if q != "y":
            input("Press enter to exit...")
            return

    # Generate private key
    private_key = rsa.generate_private_key(
        public_exponent=65537,
        key_size=4096
    )

    # Generate self-signed certificate
    subject = issuer = x509.Name([
        x509.NameAttribute(NameOID.COMMON_NAME, '2EA46.playfabapi.com'),
    ])

    cert = CertificateBuilder().subject_name(
        subject
    ).issuer_name(
        issuer
    ).public_key(
        private_key.public_key()
    ).serial_number(
        x509.random_serial_number()
    ).not_valid_before(
        datetime.datetime.utcnow()
    ).not_valid_after(
        datetime.datetime.utcnow() + datetime.timedelta(days=365)
    ).add_extension(
        x509.BasicConstraints(ca=True, path_length=None), critical=True
    ).sign(private_key, hashes.SHA256())

    # Export PKCS12 file
    from cryptography.hazmat.primitives.serialization.pkcs12 import serialize_key_and_certificates

    pfx_data = serialize_key_and_certificates(
        name=b"The Cycle: Frontier backend",
        key=private_key,
        cert=cert,
        cas=None,
        encryption_algorithm=serialization.NoEncryption()
    )

    # Save to file
    with open("certificate.pfx", "wb") as f:
        f.write(pfx_data)

    print("PKCS#12 certificate (certificate.pfx) created successfully.")
    input("Press enter to exit...")

if __name__ == '__main__':
    main()

open System
open Org.BouncyCastle.Asn1.X509
open Org.BouncyCastle.Crypto
open Org.BouncyCastle.Security
open Org.BouncyCastle.Math
open Org.BouncyCastle.Crypto.Prng
open Org.BouncyCastle.Crypto.Generators
open Org.BouncyCastle.Pkcs
open Org.BouncyCastle.X509

let kpg = RsaKeyPairGenerator()
kpg.Init(KeyGenerationParameters(SecureRandom(CryptoApiRandomGenerator()), 1024))
let kp = kpg.GenerateKeyPair()

let gen = X509V3CertificateGenerator()
let certName = X509Name("CN=PickAName")
let serialNo = BigInteger.ProbablePrime(120, new Random())

gen.SetSerialNumber(serialNo)
gen.SetSubjectDN(certName)
gen.SetIssuerDN(certName)
gen.SetNotAfter(DateTime.Now.AddYears(100))
gen.SetNotBefore(DateTime.Now.Subtract(TimeSpan(7, 0, 0, 0)))
gen.SetSignatureAlgorithm("MD5WithRSA")
gen.SetPublicKey(kp.Public)

let cert = gen.Generate(kp.Private)

let store = Pkcs12Store()
let friendlyName = cert.IssuerDN.ToString()
let entry = X509CertificateEntry(cert)
store.SetCertificateEntry(friendlyName, entry)
store.SetKeyEntry(friendlyName, AsymmetricKeyEntry(kp.Private), [| entry |])
store.Save(IO.File.OpenWrite("X509.store"), Seq.toArray "A password here", SecureRandom(CryptoApiRandomGenerator()))

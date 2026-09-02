import './Footer.css'

export default function Footer({
  brand = 'Nado, est. 2026',
  location = 'Cape Town, ZA',
  links = [
    { href: '#', label: 'Instagram' },
    { href: '#', label: 'Are.na' },
    { href: '#', label: 'LinkedIn' },
  ],
}) {
  return (
    <footer>
      <div className="wrap">
        <div className="footer-row">
          <div>{brand}</div>
          <div className="footer-links">
            {links.map((link) => (
              <a key={link.label} href={link.href}>{link.label}</a>
            ))}
          </div>
          <div>{location}</div>
        </div>
      </div>
    </footer>
  )
}
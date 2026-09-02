import Button from '../ui/Button'
import './Navbar.css'

const LINKS = [
  { href: '#work', label: 'Work' },
  { href: '#services', label: 'Services' },
  { href: '#contact', label: 'Contact' },
]

export default function Navbar({ logo = 'NADO' }) {
  return (
    <nav>
      <div className="wrap navbar-inner">
        <div className="logo">{logo}<span>.</span></div>
        <div className="navlinks">
          {LINKS.map((link) => (
            <a key={link.href} href={link.href}>{link.label}</a>
          ))}
        </div>
        <Button href="#contact" variant="primary" className="navcta">
          Start a project
        </Button>
      </div>
    </nav>
  )
}
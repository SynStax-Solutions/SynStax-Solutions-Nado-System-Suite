import { Link, useLocation } from 'react-router-dom'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'

import {
  faInstagram,
  faFacebookF,
  faLinkedinIn,
} from '@fortawesome/free-brands-svg-icons'

import { faEnvelope } from '@fortawesome/free-solid-svg-icons'

import Logo from '../../assets/Logo.svg'
import './Navbar.css'

const LINKS = [
  { path: '/', label: 'Home' },
  { path: '/about', label: 'About Us' },
  { path: '/portfolio', label: 'Our Work' },
  { path: '/contact', label: 'Contact Us' },
]

const SOCIALS = [
  {
    name: 'Instagram',
    url: '#',
    icon: faInstagram,
  },
  {
    name: 'Facebook',
    url: '#',
    icon: faFacebookF,
  },
  {
    name: 'LinkedIn',
    url: '#',
    icon: faLinkedinIn,
  },
  {
    name: 'Email',
    url: 'mailto:hello@nadographics.co.za',
    icon: faEnvelope,
  },
]

export default function Navbar() {
  const location = useLocation()

  return (
    <nav>
      <div className="wrap navbar-inner">

        {/* Logo */}
        <Link to="/" className="logo">
          <img src={Logo} alt="Nado Graphics logo" />
        </Link>

        {/* Page Navigation */}
        <div className="navlinks">
          {LINKS.map((link) => {
            const isActive =
              link.path === '/'
                ? location.pathname === '/'
                : location.pathname.startsWith(link.path)

            return (
              <Link
                key={link.path}
                to={link.path}
                className={isActive ? 'active' : ''}
              >
                {link.label}
              </Link>
            )
          })}
        </div>

        {/* Social Media */}
        <div className="social-links">
          {SOCIALS.map((social) => (
            <a
              key={social.name}
              href={social.url}
              className="social-link"
              aria-label={social.name}
              title={social.name}
            >
              <FontAwesomeIcon icon={social.icon} />
            </a>
          ))}
        </div>

      </div>
    </nav>
  )
}
import { Link, useLocation } from 'react-router-dom'
import Button from '../ui/Button'
import './Navbar.css'

const LINKS = [
  { path: '/', label: 'Home' },
  { path: '/about', label: 'About Us' },
  { path: '/portfolio', label: 'Our Work' },
  { path: '/contact', label: 'Contact Us' },
]

export default function Navbar({ logo = 'NADO' }) {
  const location = useLocation()
  return (

    <nav>
      <div className="wrap navbar-inner">
        <Link to="/" className="logo">
        {logo}<span>.</span>
        </Link>
    
        <div className="navlinks">
          {LINKS.map((link) => {
            const isActive = link.path === '/' 
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

        <Button to="/contact" variant="primary" className="navcta">
          Start a project
        </Button>
      </div>
    </nav>
  )
}
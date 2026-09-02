import './Button.css'
import { Link } from 'react-router-dom'

export default function Button({
  children, 
  to,
  href, 
  variant = 'primary', 
  onClick, 
  className = '',
  ...rest
}) {
  const classes = `btn btn-${variant} ${className}`.trim()

  if (to) {
    return (
      <Link to={to} className={classes} {...rest}>
        {children}
      </Link>
    )
  }

if (href) {
    return (
      <a href={href} className={classes} {...rest}>
        {children}
      </a>
    )
  }

  return (
  <button className={classes} onClick={onClick} {...rest}>{children}</button>

)}
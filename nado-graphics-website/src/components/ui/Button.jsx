import './Button.css'

export default function Button({
  children, href, variant = 'primary', onClick, className = '', ...rest
}) {
  const classes = `btn btn-${variant} ${className}`.trim()

  if (href) {
    return <a href={href} className={classes} {...rest}>{children}</a>
  }
  return <button className={classes} onClick={onClick} {...rest}>{children}</button>
}
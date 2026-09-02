import './SectionHead.css'

export default function SectionHead({ heading, description, voice = false }) {
  return (
    <div className="section-head">
      <h2 className={voice ? 'voice' : ''}>{heading}</h2>
      {description && <p>{description}</p>}
    </div>
  )
}
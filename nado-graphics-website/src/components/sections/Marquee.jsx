import './Marquee.css'

const DEFAULT_ITEMS = [
  { label: 'Nova Records', hot: false },
  { label: 'Kindred Coffee', hot: true },
  { label: 'Fault Line', hot: false },
  { label: 'Halcyon', hot: true },
  { label: 'Ember & Co', hot: false },
  { label: 'Loose Change', hot: true },
]

export default function Marquee({ items = DEFAULT_ITEMS }) {
  const track = [...items, ...items]
  return (
    <div className="marquee">
      <div className="marquee-track">
        {track.map((item, i) => (
          <span key={`${item.label}-${i}`} className={item.hot ? 'hot' : ''}>{item.label}</span>
        ))}
      </div>
    </div>
  )
}
var UrlLogo = '/Images/TSGH_logo_S.png';
var UrlReturn = '/Images/return.png';
var UrlLogut = '/Images/logout.png';

var isMenu = false;
if (Ext.getDoc().dom.title == 'MMSMS')
    isMenu = true;
else
    isMenu = false;

Ext.define('WEBAPP.mainBanner', {
    extend: 'Ext.Panel',
    border: 1,
    style: {
        borderColor: '#D4DDD0',
        borderStyle: 'solid'
    },
    id: 'main-banner',
    layout: {
        type: 'hbox'
    },
    defaults: {
        border: false
    },
    items: [{
        xtype: 'image',
        src: UrlLogo,
        width: '8vh',
        height: '8vh'
    }, {
        id: 'n1',
        flex: 1
    }, {
        xtype: 'image',
        src: UrlReturn,
        width: '7vh',
        height: '7vh',
        padding: '5vh 5vh 0 5vh',
        hidden: isMenu,
        listeners: {
            render: function (c) {
                c.getEl().on('click', function (e) {
                    window.location.href = '/Home/Mobile';
                }, c);
            }
        }
    }, {
        xtype: 'image',
        src: UrlLogut,
        width: '7vh',
        height: '7vh',
        padding: '5vh 5vh 0 5vh',
        listeners: {
            render: function (c) {
                c.getEl().on('click', function (e) {
                    Ext.MessageBox.confirm('登出', '確定登出？', function (btn, text) {
                        if (btn === 'yes') {
                            var UrlLogoff = '/Account/LogOff';

                            //使用一般Controller登出 2018.10.4 吉威
                            $('<form/>').attr('action', UrlLogoff)
                                .attr('method', 'POST')
                                .appendTo('body').submit().remove();
                        }
                    });
                }, c);
            }
        }
    }]
});
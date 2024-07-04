Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
   

    function getBarcodeRecord() {
        msglabel('');
        Ext.Ajax.request({
            url: '/api/CC0007/GetCrDoc',
            method: reqVal_p,
            params: {
                crdocno: T1Query.getForm().findField('CRDOCNO').getValue()
            },
            success: function (response) {

                var data = Ext.decode(response.responseText);

                if (data.success) {
                    
                    if (data.etts.length == 0) {
                        Ext.Msg.alert('提示', '查無資料，請重新輸入');
                        return;
                    }
                    var returnData = data.etts[0];

                    setItemDetail(returnData);

                    changeCardItemDetail();

                } else {
                    Ext.Msg.alert('錯誤', data.msg);
                    T1Query.getForm().findField('CRDOCNO').setValue('');
                    T1Query.getForm().findField('CRDOCNO').focus();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function setItemDetail(data) {
        var f = itemDetail.getForm();
        f.findField('CRDOCNO').setValue(data.CRDOCNO);
        f.findField('ACKMMCODE').setValue(data.ACKMMCODE);
        f.findField('PO_NO').setValue(data.PO_NO);
        f.findField('MMNAME_E').setValue(data.MMNAME_E);
        f.findField('WH_NAME').setValue(data.WH_NAME);
        f.findField('CFMQTY').setValue(data.CFMQTY);
    }

    function setData() {
        var f = itemDetail.getForm();

        myMask.show();
        
        Ext.Ajax.request({
            url: '/api/CC0007/SetData',
            method: reqVal_p,
            params: {
                crdocno: f.findField('CRDOCNO').getValue(),
                po_no: f.findField('PO_NO').getValue(),
                mmcode: f.findField('ACKMMCODE').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    myMask.hide();
                    Ext.Msg.alert('提示', data.msg);
                    return;
                }
                msglabel('三聯單' + f.findField('CRDOCNO').getValue() +'接收成功');
                
                myMask.hide();
                changeCardQuery();
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    //#region card change
    function changeCardQuery() {
        T1Query.getForm().findField('CRDOCNO').setValue('');
        //T1Query.getForm().findField('CRDOCNO').focus();
        setTimeout(function () {
            var layoutG = viewport.down('#t1Card').getLayout();
            layoutG.setActiveItem(0);
            T1Query.getForm().findField('CRDOCNO').focus();
        }, 100);
    }

    function changeCardItemDetail() {
        setTimeout(function () {
            var layout = viewport.down('#t1Card').getLayout();
            layout.setActiveItem(1);
        }, 100);
    }
    //#endregion

   
    var T1Query = Ext.widget({
        itemId: 'form',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 25%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '三聯單編號',
                            name: 'CRDOCNO',
                            id: 'CRDOCNO',
                            width: '90%',
                            labelWidth: false,
                            labelStyle: 'width: 32%',
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    if (nValue.length > 7) {
                                        //chkBarcode(nValue);
                                        getBarcodeRecord();
                                    }
                                }
                            }
                        }
                        
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    plugins: 'responsive',
                    margin: '1 0 1 0',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'hbox',
                            }
                        },
                        'width >= 600': {
                            layout: {
                                type: 'hbox',
                                vertical: false
                            }
                        }
                    },
                    items: [{
                        xtype: 'button',
                        text: '查詢',
                        id: 'query',
                        height: '9vmin',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            var f = T1Query.getForm();
                            if (!f.findField('CRDOCNO').getValue()) {
                                Ext.Msg.alert('提示', '三聯單編號為必填');
                                return;
                            }
                           
                            getBarcodeRecord();

                            msglabel('');
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        id: 'clear',
                        height: '9vmin',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('CRDOCNO').reset();
                            //f.findField('P0').setValue('');
                            //f.findField('P1').setValue('');
                            //f.findField('P2').setValue('');
                            msglabel('');
                        }
                    }]
                }
            ]
        }]
    });

    var itemDetail = Ext.widget({
        id: 'itemDetail',
        height: '100%',
        xtype: 'form',
        layout: 'form',
        closable: false,
        border: true,
        width: '100%',
        fieldDefaults: {
            xtype: 'textfield',
            //labelAlign: 'right',
            //labelWidth: 90,
            width: '100%',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 35%',
        },
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                //padding: 0,
                //margin: 0,
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '三聯單編號',
                        name: 'CRDOCNO'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '點收院內碼',
                        name: 'ACKMMCODE'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '訂單編號',
                        name: 'PO_NO'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '品名',
                        name: 'MMNAME_E',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '叫貨使用單位',
                        name: 'WH_NAME',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '訂單(結驗)數量',
                        name: 'CFMQTY',
                        readOnly: true
                    },
                   
                ]
            },
            {
                xtype: 'panel',
                border: false,
                plugins: 'responsive',
                margin: '1 0 1 0',
                responsiveConfig: {
                    'width < 600': {
                        layout: {
                            type: 'hbox',
                        }
                    },
                    'width >= 600': {
                        layout: {
                            type: 'hbox',
                            vertical: false
                        }
                    }
                },
                items: [
                {
                    xtype: 'button',
                    text: '<span style="font-size:16px">取消</span>',
                    height: '9vmin',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            width: '50%'
                        }
                    },
                    margin: '1',
                    handler: function () {
                        isCancel = true;
                        changeCardQuery();
                        T1Query.getForm().findField('CRDOCNO').setValue('');
                        T1Query.getForm().findField('CRDOCNO').focus();
                    }
                    }, {
                        xtype: 'button',
                        text: '<span style="font-size:16px">接收</span>',
                        height: '9vmin',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            var msg = '請確認進貨數量(' + itemDetail.getForm().findField('CFMQTY').getValue() + ')與三聯單紙本進貨量相符！';
                            Ext.MessageBox.confirm('', msg + '<br/>是否確認接收？', function (btn, text) {
                                if (btn === 'yes') {
                                    setData();
                                }
                            }
                            );

                            
                        }
                    }]
            }
        ]

    });


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Card',
            region: 'center',
            layout: 'card',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Query, itemDetail]
        }]
    });
    
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
});
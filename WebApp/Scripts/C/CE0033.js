﻿﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {

    var T11LastRec = null;
    var T12LastRec = null;
    var glrowIndex = null; //rowIndex
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var chk_level = '1';
    var chk_no = '';
    var chkList = [];
    var currentIndex = 0;
    var currentItem = null;

    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_E', 'MMNAME_C', 'STORE_QTYC', 'CHK_QTY,', 'STORE_LOC', 'BASE_UNIT', 'CHK_TIME', 'BARCODE']
    });

    var chknoStore = Ext.create('Ext.data.Store', {
        fields: ['CHK_NO', 'CHK_YM', 'CHK_CLASS_NAME', 'CHK_TYPE_NAME', 'CHK_STATUS_NAME', 'DISPLAY']
    });

    function setComboData() {
        //盤點人員
        Ext.Ajax.request({
            url: '/api/CE0033/GetChknoCombo/',
            method: reqVal_p,
            params:
            {
                chk_level: chk_level
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    chknoStore.removeAll();
                    var chknos = data.etts;
                    if (chknos.length > 0) {
                        for (var i = 0; i < chknos.length; i++) {
                            chknos[i].DISPLAY = (chknos[i].CHK_TYPE_NAME + ' ' + chknos[i].CHK_CLASS_NAME);
                            chknoStore.add(chknos[i]);
                        }

                    } else {
                        var chk_level_name = '';
                        if (chk_level == '1') { chk_level_name = '初盤' };
                        if (chk_level == '2') { chk_level_name = '複盤' };
                        if (chk_level == '3') { chk_level_name = '三盤' };
                        Ext.Msg.alert('提醒', '無須盤點之' + chk_level_name + '盤點單');
                    }
                    //T11Query.getForm().findField("BARCODE").focus(); // 選完盤點單並載入盤點人員後,focus在條碼
                }
            },
            failure: function (response, options) {
            }
        });

    }

    var T1Query = Ext.widget({
        itemId: 'T1Query',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        //collapsible: true,
        //hideCollapseTool: true,
        //titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 90,
            //labelStyle: 'width: 25%',
            width: '100%',
            labelStyle: "font-size:20px;font-weight:bold;",
            fieldStyle: "font-size:20px",
        },

        items: [{
            xtype: 'container',
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'displayfield',
                    fieldLabel: '月份',
                    value: getDefaultValue()
                }, {
                    xtype: 'radiogroup',
                    fieldLabel: '盤點階段',
                    name: 'CHK_LEVEL',
                    columns:2,
                    items: [
                        {
                            boxLabel: '<span style="font-size:20px">初盤</span>', name: 'CHK_LEVEL', inputValue: '1', checked: true,
                            //listeners: {
                            //    render: function (cb) {
                            //        var boxLabel = Ext.DomQuery.selectNode('LABEL', this.getEl().dom.parentNode);
                            //        boxLabel.style.fontWeight = 'bold';
                            //        boxLabel.style.fontSize = '20px';
                            //    }
                            //}
                        },
                        {  boxLabel: '<span style="font-size:20px">複盤</span>' ,name: 'CHK_LEVEL', inputValue: '2' },
                        {
                            boxLabel: '<span style="font-size:20px">三盤</span>', name: 'CHK_LEVEL', inputValue: '3'},
                    ], listeners: {                                            
                        change: function (field, newValue, oldValue) {
                            Ext.getCmp('chkStatusWarning').hide();
                            chk_level = newValue['CHK_LEVEL'];
                            setComboData();
                            T1Query.getForm().findField('CHK_NO').setValue('');
                            Ext.getCmp('btnStart').disable();

                        }
                    }
                }, {
                    xtype: 'combo',
                    store: chknoStore,
                    fieldLabel: '盤點單',
                    name: 'CHK_NO',
                    displayField: 'DISPLAY',
                    valueField: 'CHK_NO',
                    queryMode: 'local',
                    autoSelect: true,
                    fieldCls:'required',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;"><span style="font-size:20px">{DISPLAY}&nbsp;</span></div></tpl>',
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        select: function (c, r, i, e) {
                            
                            Ext.getCmp('btnStart').disable();
                            Ext.getCmp('chkStatusWarning').hide();
                            if (r.data.CHK_STATUS == '1') {
                                
                                Ext.getCmp('btnStart').enable();
                                return;
                            }

                            Ext.getCmp('chkStatusWarning').show();
                        },
                        focus: function (field, event, eOpts) {
                            
                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 100);
                            }
                        }
                    }
                },
                {
                    xtype: 'box',
                    width: '100%',
                    id: 'chkStatusWarning',
                    hidden: true,
                    renderTpl: [
                        '<table width="100%"><tr><td align="center">'
                        + '<span style="font-size:1.5em;">已完成盤點，不可輸入</span>'
                        + '</td></tr></table>'
                    ],
                    width: '100%'
                }, {
                        xtype: 'button',
                        text: '開始盤點',
                        id:'btnStart',
                        //width: '15%',
                        disabled:true,
                        margin: '8 0 0 8',
                        handler: function () {
                            mask.show();


                            var chkTypeString = '';
                            if (chk_level == '1') { chkTypeString += '初盤 ' }
                            if (chk_level == '2') { chkTypeString += '複盤 ' }
                            if (chk_level == '3') { chkTypeString += '三盤 ' }
                            chkTypeString += T1Query.getForm().findField('CHK_NO').rawValue;
                            itemDetail.getForm().findField('itemDetailChkType').setValue(chkTypeString);
                            itemDetail.getForm().findField('CHK_QTY').setValue('');
                            itemDetail.getForm().findField('STORE_LOC').setValue('');

                            itemDetail.getForm().findField('BARCODE').show();
                            itemDetail.getForm().findField('CHK_QTY').hide();

                            changeCardItemDetail();
                            T1Load();
                           
                        }
                }
            ]
        }
        ]
    });

    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;
        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;
        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        return yyyy.toString() + mm;
    }

    function T1Load() {
        Ext.Ajax.request({
            url: '/api/CE0033/GetUserChkList/',
            method: reqVal_p,
            params:
            {
                chk_no: T1Query.getForm().findField('CHK_NO').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    chkList = data.etts;
                    
                    setItemDetail('first');
                    //T11Query.getForm().findField("BARCODE").focus(); // 選完盤點單並載入盤點人員後,focus在條碼
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function setItemDetail(actionType) {
        
        Ext.getCmp('btnQueryItem').show();
        Ext.getCmp('btnCheck').hide();
        
        if (actionType == 'pre') {
            setItemDetailPre();

            return;
        }
        if (actionType == 'first') {
            setItemDetailFirst();
            return;
        }

        if (currentIndex >= chkList.length || currentIndex < 0) {
            Ext.getCmp('finishEntryMsg').show();
            Ext.getCmp('detailInfo').hide();

            Ext.getCmp('btnPreItem').disable();

            var f = itemDetail.getForm();
            f.findField('STORE_LOC').setValue('');
            f.findField('BARCODE').setValue('');
            f.findField('CHK_QTY').setValue('');
            f.findField('BARCODE').show();
            f.findField('CHK_QTY').hide();

            return;

        }

        Ext.getCmp('finishEntryMsg').hide();
        Ext.getCmp('detailInfo').show();
        
        if (currentIndex > 0) {
            Ext.getCmp('btnPreItem').enable();
        }

        var f = itemDetail.getForm();
        f.findField('STORE_LOC').setValue(chkList[currentIndex].STORE_LOC);
        f.findField('MMCODE').setValue(chkList[currentIndex].MMCODE);
        f.findField('MMNAME_C').setValue(chkList[currentIndex].MMNAME_C);
        f.findField('MMNAME_E').setValue(chkList[currentIndex].MMNAME_E);
        f.findField('BASE_UNIT').setValue(chkList[currentIndex].BASE_UNIT);
        f.findField('CHK_TIME').setValue(chkList[currentIndex].CHK_TIME);

        f.findField('BARCODE').setValue('');
        f.findField('BARCODE').show();
        f.findField('CHK_QTY').hide();

        setTimeout(function () { f.findField("BARCODE").focus(); }, 300);
        mask.hide();

        T1Load();
        
    }

    function setItemDetailPre() {
        
        var f = itemDetail.getForm();

        Ext.getCmp('finishEntryMsg').hide();
        Ext.getCmp('detailInfo').show();

        Ext.getCmp('btnQueryItem').show();
        Ext.getCmp('btnCheck').hide();
        f.findField("BARCODE").show();
        f.findField("CHK_QTY").hide();

        f.findField('STORE_LOC').setValue(chkList[currentIndex].STORE_LOC);
        f.findField('MMCODE').setValue(chkList[currentIndex].MMCODE);
        f.findField('MMNAME_C').setValue(chkList[currentIndex].MMNAME_C);
        f.findField('MMNAME_E').setValue(chkList[currentIndex].MMNAME_E);
        f.findField('BASE_UNIT').setValue(chkList[currentIndex].BASE_UNIT);
        f.findField('CHK_QTY').setValue(chkList[currentIndex].CHK_QTY);
        f.findField('CHK_TIME').setValue(chkList[currentIndex].CHK_TIME);

        f.findField("BARCODE").setValue('');
        f.findField("BARCODE").focus();

        if (currentIndex == 0) {
            Ext.getCmp('btnPreItem').disable();
        } else {
            Ext.getCmp('btnPreItem').enable();
        }
    }

    function setItemDetailFirst() {
        
        currentIndex = 0;
        var isDone = true;
        for (var i = 0; i < chkList.length; i++) {
            if (chkList[i].CHK_TIME == '') {
                Ext.getCmp('finishEntryMsg').hide();
                Ext.getCmp('detailInfo').show();

                currentIndex = i;
                if (currentIndex > 0) {
                    Ext.getCmp('btnPreItem').enable();
                } else {
                    Ext.getCmp('btnPreItem').disable();
                }
                isDone = false;

                var f = itemDetail.getForm();
                f.findField('STORE_LOC').setValue(chkList[currentIndex].STORE_LOC);
                f.findField('MMCODE').setValue(chkList[currentIndex].MMCODE);
                f.findField('MMNAME_C').setValue(chkList[currentIndex].MMNAME_C);
                f.findField('MMNAME_E').setValue(chkList[currentIndex].MMNAME_E);
                f.findField('BASE_UNIT').setValue(chkList[currentIndex].BASE_UNIT);
                f.findField('CHK_TIME').setValue(chkList[currentIndex].CHK_TIME);

                mask.hide();
                f.findField("BARCODE").setValue('');

                setTimeout(function () { f.findField("BARCODE").focus(); }, 300);
                
                return;
            }
        }

        if (isDone) {
            
            currentIndex = -1;

            Ext.getCmp('finishEntryMsg').show();
            Ext.getCmp('detailInfo').hide();
            Ext.getCmp('btnPreItem').disable();
            itemDetail.getForm().findField("STORE_LOC").setValue('');
            itemDetail.getForm().findField("BARCODE").setValue('');
            itemDetail.getForm().findField("CHK_QTY").setValue('');
            itemDetail.getForm().findField('BARCODE').show();
            itemDetail.getForm().findField('CHK_QTY').hide();

            mask.hide();
        }
    }

    function DataLoad() {
        
        var f = itemDetail.getForm();

        var newIndex = -1;
        
        for (var i = 0; i < chkList.length; i++) {
            if (chkList[i].STORE_LOC == f.findField('STORE_LOC').getValue().trim() &&
                chkList[i].MMCODE == f.findField('BARCODE').getValue().trim()) {
                newIndex = i;
                break;
            }
            if (chkList[i].STORE_LOC == f.findField('STORE_LOC').getValue().trim() &&
                chkList[i].MMCODE == f.findField('MMCODE').getValue().trim()) {
                newIndex = i;
                break;
            }
        }
        
        if (newIndex < 0) {
            f.findField('STORE_LOC').setValue('');
            f.findField('BARCODE').setValue('');
            f.findField('CHK_QTY').setValue('');
            
            Ext.Msg.alert('提醒', '儲位碼輸入錯誤，請重新輸入');
            return;
        }

        currentIndex = newIndex;
        Ext.Ajax.request({
            url: '/api/CE0033/GetItemDetail/',
            method: reqVal_p,
            params:
            {
                chk_no: T1Query.getForm().findField('CHK_NO').getValue(),
                store_loc: chkList[currentIndex].STORE_LOC,
                mmcode: chkList[currentIndex].MMCODE,
                barcode: itemDetail.getForm().findField('BARCODE').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.etts.length > 0) {
                        currentItem = data.etts[0];
                        f.findField('MMCODE').setValue(currentItem.MMCODE);
                        f.findField('MMNAME_C').setValue(currentItem.MMNAME_C);
                        f.findField('MMNAME_E').setValue(currentItem.MMNAME_E);
                        f.findField('BASE_UNIT').setValue(currentItem.BASE_UNIT);
                        f.findField('CHK_QTY').setValue(currentItem.CHK_QTY);
                        f.findField('CHK_TIME').setValue(currentItem.CHK_TIME);

                        f.findField('CHK_QTY').show();
                        f.findField('BARCODE').hide();

                        f.findField("CHK_QTY").focus();

                        Ext.getCmp('btnQueryItem').hide();
                        Ext.getCmp('btnCheck').show();

                        Ext.getCmp('finishEntryMsg').hide();
                        Ext.getCmp('detailInfo').show();
                        if (currentIndex == 0) {
                            Ext.getCmp('btnPreItem').disable();
                        }

                    } else {
                        //f.findField('MMCODE').setValue('');
                        //f.findField('MMNAME_C').setValue('');
                        //f.findField('MMNAME_E').setValue('');
                        //f.findField('BASE_UNIT').setValue('');
                        f.findField('BARCODE').setValue('');
                        Ext.Msg.alert('提醒', data.msg, function () { f.findField('BARCODE').focus();});
                    }
                    
                }
            },
            failure: function (response, options) {
            }
        });
    }

    var itemDetail = Ext.create('Ext.form.Panel', {
        id: 'itemDetail',
        height: '100%',
        layout: 'fit',
        closable: false,
        border: true,
        width: '100%',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 90,
            width: '100%',
            padding: 0,
            margin: 0,
            labelStyle: "font-size:20px;font-weight:bold;",
            fieldStyle: "font-size:20px",
        },
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                //padding: 0,
                //margin: 0,
                items: [
                    {
                        xtype: 'container',
                        layout: 'vbox',
                        padding: '2vmin',
                        width: '100%',
                        items: [
                            {
                                xtype: 'button',
                                text: '<span style="font-size:16px">重新選擇盤點單</span>',
                                id: 'btnBackQuery',
                                //width: '15%',
                                margin: '0 0 0 8',
                                handler: function () {
                                    changeCardQuery();
                                }

                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '盤點月份',
                                value: getDefaultValue(),
                                
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '盤點類別',
                                name:'itemDetailChkType',
                                value: ''
                            },
                        ]
                    },
                    {
                        xtype: 'box',
                        autoEl: { tag: 'hr' },
                        width:'100%'
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '物料儲位',
                        id:'store_loc',
                        name: 'STORE_LOC',
                        listeners: {
                            blur: function (field, nValue, oValue, eOpts) {
                            }
                        }
                    }, 
                    {
                        xtype: 'textfield',
                        fieldLabel: '物料條碼',
                        name: 'BARCODE',
                        id: 'barcode',
                        listeners: {
                            keydown: function (field, e, eOpts) {
                                if (e.keyCode == '13') {
                                    Ext.getCmp('btnQueryItem').click();
                                }
                            }
                        }
                    }, 
                    {
                        xtype: 'numberfield',
                        fieldLabel: '盤點量',
                        name: 'CHK_QTY',
                        hideTrigger: true,
                        hidden:true,
                        minValue:0,
                        
                    }, 
                    {
                        xtype: 'container',
                        layout: 'hbox',
                        right: '100%',
                        width: '100%',
                        items: [
                            {
                                xtype: 'button',
                                text: '<span style="font-size:16px">前一筆</span>',
                                id: 'btnPreItem',
                                //width: '15%',
                                margin: '0 0 0 8',
                                handler: function () {
                                    
                                    currentIndex = currentIndex - 1;
                                    setItemDetail('pre');
                                }

                            },
                            {
                                xtype: 'component',
                                flex: 1
                            },
                            {
                                xtype: 'button',
                                text: '<span style="font-size:16px">查詢</span>',
                                id: 'btnQueryItem',
                                //width: '15%',
                                margin: '0 0 0 8',
                                handler: function () {
                                    if (!itemDetail.getForm().findField('STORE_LOC').getValue() ||
                                        !itemDetail.getForm().findField('BARCODE').getValue()) {
                                        Ext.Msg.alert('提醒', '<span style="font-size:20px"><span style="color:red;">儲位</span>與<span style="color:red;">物料條碼</span>皆須輸入</span>');
                                        return 
                                    }
                                    msglabel('');
                                    DataLoad();
                                }

                            },
                            {
                                xtype: 'button',
                                text: '<span style="font-size:16px">確定</span>',
                                id: 'btnCheck',
                                hidden: true,
                                //width: '15%',
                                margin: '0 0 0 8',
                                handler: function () {
                                    
                                    if (itemDetail.getForm().findField('CHK_QTY').getValue() == null) {
                                        Ext.Msg.alert('提醒', '請輸入盤點量');
                                        return 
                                    }
                                    if (isNaN(Number(itemDetail.getForm().findField('CHK_QTY').getValue()))) {
                                        Ext.Msg.alert('提醒', '盤點量只能輸入數字');
                                        return 
                                    }
                                    setChkqty();
                                }

                            },
                        ]
                    },
                    {
                        xtype: 'box',
                        autoEl: { tag: 'hr' },
                        width: '100%'
                    },
                    {
                        xtype: 'box',
                        width: '100%',
                        id:'finishEntryMsg',
                        renderTpl: [
                            '<table width="100%"><tr><td align="center">'
                            + '<span style="font-size:3em; color:red">已完成輸入</span>'
                            +'</td></tr></table>'
                        ],
                        width: '100%'
                    },
                    {
                        xtype: 'container',
                        layout: 'vbox',
                        id: 'detailInfo',
                        width: '100%',
                        items: [
                            {
                                xtype: 'displayfield',
                                fieldLabel: '院內碼',
                                name: 'MMCODE',
                                readOnly: true
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '中文品名',
                                name: 'MMNAME_C',
                                readOnly: true
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '院內碼',
                                name: 'MMCODE',
                                readOnly: true,
                                hidden:true
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '英文品名',
                                name: 'MMNAME_E',
                                readOnly: true
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '計量單位',
                                name: 'BASE_UNIT',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '盤點時間',
                                name: 'CHK_TIME',
                                readOnly: true,
                                renderer: function (value, meta, record) {
                                    if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (Taipei Standard Time)") {  //value值是 null時候
                                        return '';
                                    } else if (value == "Mon Jan 01 0001 00:00:00 GMT+0806 (台北標準時間)") {
                                        return '';
                                    } else {
                                        return Ext.util.Format.date(value, 'Xmd H:i:s');
                                    }
                                },
                            }, {
                                xtype: 'hidden',
                                name: 'CHK_NO',
                                submitValue: true
                            }
                        ]
                    },
                    
                    
                ]
            }
        ]
    });

    function setChkqty() {
        Ext.Ajax.request({
            url: '/api/CE0033/SetChkqty/',
            method: reqVal_p,
            params:
            {
                chk_no: chkList[currentIndex].CHK_NO,
                store_loc: chkList[currentIndex].STORE_LOC,
                mmcode: chkList[currentIndex].MMCODE,
                chk_qty: itemDetail.getForm().findField('CHK_QTY').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    msglabel('已更新盤點量');
                    currentIndex = currentIndex + 1;
                    if (currentIndex == chkList.length) {
                        currentIndex = -1;
                        itemDetail.getForm().findField('STORE_LOC').setValue('');
                        itemDetail.getForm().findField('BARCODE').setValue('');
                        itemDetail.getForm().findField('CHK_QTY').setValue('');
                        Ext.getCmp('btnQueryItem').show();
                        Ext.getCmp('btnCheck').hide();
                        Ext.getCmp('btnPreItem').disable();
                    }
                    setItemDetail();
                    //T11Query.getForm().findField("BARCODE").focus(); // 選完盤點單並載入盤點人員後,focus在條碼
                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
            }
        });
    }
    
    
    Ext.on('resize', function () {
        windowHeight = $(window).height();
        windowWidth = $(window).width();
    });

    function changeCardQuery() {
        setTimeout(function () {
            var layoutG = viewport.down('#t1Card').getLayout();
            layoutG.setActiveItem(0);
        }, 100);
    }

    function changeCardItemDetail() {
        setTimeout(function () {
            var layout = viewport.down('#t1Card').getLayout();
            layout.setActiveItem(1);
        }, 100);
    }

    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 網頁進入點 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            id: 't1Card',
            region: 'center',
            layout: 'card',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Query, itemDetail]
        }]
    });
    var mask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    setComboData();

});
